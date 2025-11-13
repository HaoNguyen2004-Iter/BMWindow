using Microsoft.EntityFrameworkCore;
namespace DBContext.BMWindows.Entities
{
    public class BMWindowDBContext : DbContext
    {
        public BMWindowDBContext(DbContextOptions<BMWindowDBContext> options)
            : base(options) { }

        public DbSet<Category> Categories => Set<Category>();
        public DbSet<AppItem> AppItems => Set<AppItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            // ===== Categories =====
            modelBuilder.Entity<Category>(entity =>
            {
                // Khai báo trigger cho bảng Categories
                entity.ToTable("Categories", tb =>
                {
                    tb.HasTrigger("TR_Categories_SetUpdateTime");
                    // Nếu có nhiều trigger, thêm nhiều dòng HasTrigger(...)
                });

                entity.HasKey(e => e.Id).HasName("PK_Categories");

                entity.Property(e => e.Name)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(e => e.Prioritize)
                      .IsRequired();

                entity.Property(e => e.CreateBy)
                      .IsRequired()
                      .HasDefaultValue(1);

                entity.Property(e => e.CreateTime)
                      .HasColumnType("datetime2(0)")
                      .HasDefaultValueSql("SYSDATETIME()");

                entity.Property(e => e.Status)
                      .HasColumnType("tinyint")
                      .HasDefaultValue((byte)1);

                entity.Property(e => e.Keyword)
                      .HasMaxLength(400)
                      .HasDefaultValue(string.Empty);

                entity.Property(e => e.UpdateTime)
                      .HasColumnType("datetime2(0)")
                      .HasDefaultValueSql("SYSDATETIME()");

                entity.Property(e => e.UpdateBy)
                      .IsRequired()
                      .HasDefaultValue(1);

                entity.HasIndex(e => e.Status).HasDatabaseName("IX_Categories_Status");
                entity.HasIndex(e => e.Keyword).HasDatabaseName("IX_Categories_Keyword");
            });

            // ===== AppItems =====
            modelBuilder.Entity<AppItem>(entity =>
            {
                // Khai báo trigger cho bảng AppItems
                entity.ToTable("AppItems", tb =>
                {
                    tb.HasTrigger("TR_AppItems_SetUpdateTime");
                });

                entity.HasKey(e => e.Id).HasName("PK_AppItems");

                entity.Property(e => e.CategoryId)
                      .IsRequired();

                entity.Property(e => e.Name)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(e => e.Icon)
                      .HasMaxLength(200);

                entity.Property(e => e.Size)
                      .HasMaxLength(50);

                entity.Property(e => e.Url)
                      .HasMaxLength(500);

                entity.Property(e => e.Token)
                      .HasMaxLength(200); // nvarchar(200) NULL

                entity.Property(e => e.Expired); // int? NULL

                entity.Property(e => e.AppExpire)
                      .HasColumnType("datetime2(0)"); // datetime2(0) NULL

                entity.Property(e => e.Prioritize)
                      .IsRequired();

                entity.Property(e => e.CreateBy)
                      .IsRequired()
                      .HasDefaultValue(1);

                entity.Property(e => e.CreateTime)
                      .HasColumnType("datetime2(0)")
                      .HasDefaultValueSql("SYSDATETIME()");

                entity.Property(e => e.Status)
                      .HasColumnType("tinyint")
                      .HasDefaultValue((byte)1);

                entity.Property(e => e.Keyword)
                      .HasMaxLength(400)
                      .HasDefaultValue(string.Empty);

                entity.Property(e => e.UpdateTime)
                      .HasColumnType("datetime2(0)")
                      .HasDefaultValueSql("SYSDATETIME()");

                entity.Property(e => e.UpdateBy)
                      .IsRequired()
                      .HasDefaultValue(1);

                entity.HasIndex(e => e.CategoryId).HasDatabaseName("IX_AppItems_CategoryId");
                entity.HasIndex(e => e.Status).HasDatabaseName("IX_AppItems_Status");
                entity.HasIndex(e => e.Keyword).HasDatabaseName("IX_AppItems_Keyword");
                entity.HasIndex(e => e.Prioritize)
                      .IsUnique()
                      .HasDatabaseName("UX_AppItems_Prioritize");
            });
        }
    }
}