namespace DBContext.BMWindows.Entities
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int Prioritize { get; set; }   
        public int CreateBy { get; set; }
        public DateTime CreateTime { get; set; }
        public byte Status { get; set; }
        public string Keyword { get; set; } = string.Empty;
        public DateTime UpdateTime { get; set; }
        public int UpdateBy { get; set; }
    }
}
