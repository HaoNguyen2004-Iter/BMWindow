using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

        // Bất đồng bộ hóa
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
                    CreateBy = x.CreateBy,
                    CreateTime = x.CreateTime,
                    UpdateBy = x.UpdateBy,
                    UpdateTime = x.UpdateTime,
                    Prioritize = x.Prioritize,
                    Keyword = x.Keyword
                })
                .FirstOrDefaultAsync();

            if (result == null)
                throw new KeyNotFoundException($"Không tìm thấy danh mục với id = {id}");

            return result;
        }
    }
}