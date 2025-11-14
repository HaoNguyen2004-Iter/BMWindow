using Microsoft.EntityFrameworkCore;

namespace Service.BMWindows.Executes.Category
{
    public class CategoryOne
    {
        private readonly DBContext.BMWindows.Entities.BMWindowDBContext _context;

        public CategoryOne(DBContext.BMWindows.Entities.BMWindowDBContext context)
        {
            _context = context;
        }

        public async Task<CategoryModel> GetOneCategory(int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException("Lỗi khi tìm danh mục");

            var result = await _context.Categories
                .Where(x => x.Id == id)
                .Select(x => new CategoryModel
                {
                    Id = x.Id,
                    Status = x.Status,
                    Name = x.Name,
                    Prioritize = x.Prioritize,
                    CreatedBy = x.CreatedBy,
                    CreatedDate = x.CreatedDate,
                    UpdatedBy = x.UpdatedBy ?? Guid.Empty,
                    UpdatedDate = x.UpdatedDate,
                    Keyword = x.Keyword
                })
                .FirstOrDefaultAsync();

            if (result == null)
                throw new KeyNotFoundException($"Không tìm thấy danh mục với id = {id}");

            return result;
        }
    }
}