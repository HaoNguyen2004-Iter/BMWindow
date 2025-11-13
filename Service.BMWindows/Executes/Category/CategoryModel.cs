namespace Service.BMWindows.Executes.Category
{
    public class CategoryModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int CreateBy { get; set; }
        public DateTime CreateTime { get; set; }
        public byte Status { get; set; }
        public string Keyword { get; set; } = string.Empty;
        public DateTime UpdateTime { get; set; }
        public int UpdateBy { get; set; }
        public int Prioritize { get; set; }


        public DateTime? CreateDateFrom { get; set; }
        public DateTime? CreateDateTo { get; set; }
        public DateTime? UpdateDateFrom { get; set; }
        public DateTime? UpdateDateTo { get; set; }
    }
}
