using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

public static class SqlGuard
{
    private static readonly string[] DangerousPatterns = {
        @"\b(SELECT|INSERT|UPDATE|DELETE|DROP|ALTER|CREATE|TRUNCATE|EXEC|UNION|BACKUP|RESTORE|GRANT|REVOKE|DENY|KILL|SHUTDOWN)\b",
        @"(--|#|/\*|\*/|;)",
        @"\bUNION\b\s+.*\bSELECT\b",
        @"\b(?:OR|AND)\s+1\s*=\s*1\b",
        @"'\s*(?:OR|AND)\s*'\w+'?\s*=\s*'\w+'?",
        @"\b(?:xp_|sp_)\w+",
        @"\bWAITFOR\s+DELAY\b|\bSLEEP\s*\(",
        @";\s*\b(?:SELECT|INSERT|UPDATE|DELETE|DROP|ALTER|CREATE|TRUNCATE|EXEC)\b"
    };

    private static readonly RegexOptions Options =
        RegexOptions.IgnoreCase | RegexOptions.Compiled;

    /// <summary>
    /// Kiểm tra đầu vào có mã SQL nguy hiểm không.
    /// </summary>
    public static bool IsSuspicious(object? model, int maxDepth = 3)
    {
        if (model == null) return false;

        // Tạo visited theo từng lần gọi để tránh đụng độ giữa các request
        var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);
        return Scan(model, maxDepth, visited);
    }

    // Hàm quét (truyền visited theo luồng kiểm tra)
    private static bool Scan(object obj, int depth, HashSet<object> visited)
    {
        if (obj == null || depth < 0 || !visited.Add(obj)) return false;

        var type = obj.GetType();

        foreach (var prop in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            if (!prop.CanRead) continue;

            object? value;
            try { value = prop.GetValue(obj); }
            catch { continue; }

            if (value is string s && !string.IsNullOrWhiteSpace(s))
            {
                var clean = Clean(s);
                foreach (var pattern in DangerousPatterns)
                {
                    if (Regex.IsMatch(clean, pattern, Options))
                        return true;
                }
            }
            else if (value is IEnumerable list && value is not string)
            {
                foreach (var item in list)
                    if (item != null && Scan(item, depth - 1, visited))
                        return true;
            }
            else if (value != null && !IsSimpleType(prop.PropertyType))
            {
                if (Scan(value, depth - 1, visited))
                    return true;
            }
        }
        return false;
    }

    // Dọn dẹp chuỗi
    private static string Clean(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return string.Empty;

        var halfWidth = new StringBuilder();
        foreach (var c in s)
        {
            if (c >= 0xFF01 && c <= 0xFF5E)
                halfWidth.Append((char)(c - 0xFEE0));
            else if (c == 0x3000)
                halfWidth.Append(' ');
            else
                halfWidth.Append(c);
        }

        var normalized = halfWidth.ToString();

        return Regex.Replace(
            Regex.Replace(normalized, @"\p{C}+", " "),
            @"\s+", " ").Trim();
    }

    // Kiểu dữ liệu đơn giản
    private static bool IsSimpleType(Type t)
    {
        t = Nullable.GetUnderlyingType(t) ?? t;
        return t.IsPrimitive || t.IsEnum || t == typeof(string) ||
               t == typeof(decimal) || t == typeof(DateTime) ||
               t == typeof(DateTimeOffset) || t == typeof(Guid) || t == typeof(TimeSpan);
    }
}