const fs = require("fs");
const fsp = require("fs/promises");
const path = require("path");

const args = process.argv.slice(2);
if (!args[0]) {
    console.error("Thiếu <FolderName>. Ví dụ: node make-cs-scaffold.js Uploads --base Upload");
    process.exit(1);
}
const folderNameRaw = args[0];

function opt(flag, def = null) {
    const i = args.indexOf(flag);
    return i !== -1 && args[i + 1] ? args[i + 1] : def;
}

const baseNameRaw = opt("--base", folderNameRaw);
const nsRootArg = opt("--nsroot", null);
const outOpt = opt("--out", null);
const serviceHint = opt("--service", null); // ví dụ: BMWindows hoặc Service.BMWindows
const force = args.includes("--force");

function toPascal(s) {
    return (s || "")
        .replace(/[_\-\.\s]+/g, " ")
        .trim()
        .split(" ")
        .filter(Boolean)
        .map(w => w[0].toUpperCase() + w.slice(1))
        .join("");
}

function isDir(p) {
    try { return fs.statSync(p).isDirectory(); } catch { return false; }
}

function subdirs(p) {
    try {
        return fs.readdirSync(p, { withFileTypes: true })
            .filter(d => d.isDirectory())
            .map(d => d.name);
    } catch {
        return [];
    }
}

function sanitizeNs(s) {
    return (s || "").replace(/[^A-Za-z0-9_.]/g, "").replace(/^\.+|\.+$/g, "");
}

// Suy luận nsRoot từ đường dẫn serviceDir
function deriveNsRoot(serviceDir) {
    const base = path.basename(serviceDir);
    const parent = path.basename(path.dirname(serviceDir));
    if (/^Service\./.test(base)) return sanitizeNs(base);
    if (parent === "Service") return sanitizeNs(`Service.${toPascal(base)}`);
    return sanitizeNs(base);
}

// Tìm Service.* gần nhất và thư mục Executes tương ứng
function findServiceExecutes(startDir, hint) {
    let cur = startDir;

    const matchesHint = (name, hintVal) => {
        if (!hintVal) return false;
        return name === hintVal || name === `Service.${hintVal}`;
    };

    while (true) {
        const dirs = subdirs(cur);
        const candidates = [];

        // 1) Nếu có thư mục "Service" ở cấp này, duyệt bên trong
        if (dirs.includes("Service")) {
            const serviceBase = path.join(cur, "Service");
            for (const name of subdirs(serviceBase)) {
                const serviceDir = path.join(serviceBase, name);
                const executesDir = path.join(serviceDir, "Executes");
                if (isDir(executesDir)) {
                    candidates.push({ name, serviceDir, executesDir });
                }
            }
        }

        // 2) Duyệt các thư mục "Service.*" ngay dưới cấp hiện tại
        for (const name of dirs) {
            if (/^Service\./.test(name)) {
                const serviceDir = path.join(cur, name);
                const executesDir = path.join(serviceDir, "Executes");
                if (isDir(executesDir)) {
                    candidates.push({ name, serviceDir, executesDir });
                }
            }
        }

        // 3) Nếu chính cur là "Service.*"
        const curName = path.basename(cur);
        if (/^Service\./.test(curName)) {
            const executesDir = path.join(cur, "Executes");
            if (isDir(executesDir)) {
                candidates.push({ name: curName, serviceDir: cur, executesDir });
            }
        }

        if (candidates.length > 0) {
            // Ưu tiên theo --service nếu có
            let chosen = null;
            if (hint) {
                chosen = candidates.find(c => matchesHint(c.name, hint));
                if (!chosen) {
                    // Cho phép hint khớp với phần sau "Service."
                    const lowered = hint.toLowerCase();
                    chosen = candidates.find(c =>
                        c.name.toLowerCase() === lowered ||
                        c.name.toLowerCase() === `service.${lowered}`
                    );
                }
            }
            if (!chosen) {
                // Nếu chỉ có một ứng viên, chọn nó. Nếu nhiều, chọn theo tên alpha.
                candidates.sort((a, b) => a.name.localeCompare(b.name));
                chosen = candidates[0];
                if (candidates.length > 1) {
                    const list = candidates.map(c => c.name).join(", ");
                    console.warn(`ℹ️ Tìm thấy nhiều Service có Executes: ${list}. Đã chọn: ${chosen.name}. (dùng --service <Name> để chỉ rõ)`);
                }
            }
            return chosen;
        }

        const parent = path.dirname(cur);
        if (parent === cur) break;
        cur = parent;
    }

    return null;
}

const cwd = process.cwd();

let executesBaseDir = null;
let nsRoot = nsRootArg;

// Nếu --out được truyền, dùng nó làm thư mục Service project. Tạo/đặt Executes bên trong.
if (outOpt) {
    const absOut = path.resolve(cwd, outOpt);
    const execDir = path.join(absOut, "Executes");
    executesBaseDir = execDir;
    if (!nsRoot) {
        nsRoot = deriveNsRoot(absOut);
    }
} else {
    // Tự động tìm Service.* gần nhất có Executes
    const found = findServiceExecutes(cwd, serviceHint);
    if (!found) {
        console.error("Không tìm thấy thư mục Service.* có Executes ở các cấp cha. Hãy dùng --out <ServiceProjectPath> hoặc tạo thư mục Executes trước.");
        process.exit(1);
    }
    executesBaseDir = found.executesDir;
    if (!nsRoot) {
        nsRoot = deriveNsRoot(found.serviceDir);
    }
    console.log(`🔎 Đã chọn service: ${path.relative(cwd, found.serviceDir)}`);
    console.log(`📌 Executes: ${path.relative(cwd, executesBaseDir)}`);
}

const folderName = toPascal(folderNameRaw);
const baseName = toPascal(baseNameRaw);
nsRoot = sanitizeNs(nsRoot);
const namespaceFull = `${nsRoot}.Executes.${folderName}`;

const targetDir = path.join(executesBaseDir, folderName);

const suffixes = ["One", "Model", "Many", "Command"];

function csTemplate(className, ns) {
    return `namespace ${ns}
{
    public class ${className}
    {

    }
}
`;
}

(async () => {
    try {
        await fsp.mkdir(targetDir, { recursive: true });
        for (const sfx of suffixes) {
            const className = `${baseName}${sfx}`;
            const fp = path.join(targetDir, `${className}.cs`);
            if (fs.existsSync(fp) && !force) {
                console.warn(`⚠️  Tồn tại: ${path.relative(cwd, fp)} (dùng --force để ghi đè)`);
                continue;
            }
            await fsp.writeFile(fp, csTemplate(className, namespaceFull), "utf8");
            console.log(`✅ Tạo: ${path.relative(cwd, fp)}`);
        }

        console.log(`\n📁 Thư mục: ${path.relative(cwd, targetDir)}`);
        console.log(`🧩 Namespace: ${namespaceFull}`);
    } catch (e) {
        console.error("Lỗi:", e.message);
        process.exit(1);
    }
})();

//Lệnh mẫu
//node Web.BMWindows/wwwroot/assets/js/make-cs-scaffold.js(Chỗ file nằm) "Tên thư mục"" --service BMWindows(Tên dự án). Auto vào Service/Execute
