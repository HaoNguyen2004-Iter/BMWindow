namespace DBContext.BMWindows.Entities
{
    public class AppItem
    {
        public int Id { get; set; }

        public int CategoryId { get; set; }   // soft FK
        public string Name { get; set; } = null!;
        public string? Icon { get; set; }
        public string? Size { get; set; }
        public string? Url { get; set; }
        public string? Token { get; set; }    // nvarchar(200) NULL
        public int? Expired { get; set; }     
        public DateTime? AppExpire { get; set; } // datetime2(0) NULL
        public int Prioritize { get; set; }   // required, unique
        public int CreateBy { get; set; }
        public DateTime CreateTime { get; set; }
        public byte Status { get; set; }
        public string Keyword { get; set; } = string.Empty;
        public DateTime UpdateTime { get; set; }
        public int UpdateBy { get; set; }
    }
}
