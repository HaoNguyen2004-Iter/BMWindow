namespace DBContext.BMWindows.Entities
{
    public class AppItem
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }   
        public string Name { get; set; } = null!;
        public string? Icon { get; set; }
        public string? Size { get; set; }
        public string? Url { get; set; }
        public int Status { get; set; }
        public string? Keyword { get; set; }
        public int Prioritize  { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public Guid UpdatedBy { get; set; }
    }
}
