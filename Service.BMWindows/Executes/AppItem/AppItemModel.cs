using System;

namespace Service.BMWindows.Executes.AppItem
{
    // Khóa mềm: giữ CategoryId + CategoryName (snapshot) nhưng KHÔNG tạo FK cứng ở DB.
    // Service sẽ kiểm tra tồn tại Category trước khi Add/Update.
    public class AppItemModel
    {
        public int Id { get; set; }

        // Soft reference (ràng buộc bằng logic, không bằng FK)
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }          // lưu snapshot tên để hiển thị nhanh

        public string Name { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public string? Size { get; set; }
        public string? Url { get; set; }
        public int Prioritize { get; set; }
        public int Status { get; set; }                    // 1: active, 0: inactive
        public string? Keyword { get; set; }

        public Guid CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }

        // Filter helpers
        public DateTime? CreateDateFrom { get; set; }
        public DateTime? CreateDateTo { get; set; }
        public DateTime? UpdateDateFrom { get; set; }
        public DateTime? UpdateDateTo { get; set; }
    }
}