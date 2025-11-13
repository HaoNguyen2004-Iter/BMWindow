using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SPMH.Services.Utils;
using Service.BMWindows.Variables;

namespace Service.BMWindows.Executes.Category
{
    public class CategoryCommand
    {
        private readonly DBContext.BMWindows.Entities.BMWindowDBContext _context;

        public CategoryCommand(DBContext.BMWindows.Entities.BMWindowDBContext context)
        {
            _context = context;
        }

        public async Task<CategoryModel> AddCategory(CategoryModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (string.IsNullOrWhiteSpace(model.Name)) throw new ArgumentException("Tên danh mục không hợp lệ", nameof(model));
            if (SqlGuard.IsSuspicious(model)) throw new Exception("Đầu vào không hợp lệ");

            // Đảm bảo Prioritize là duy nhất (nếu yêu cầu unique)
            if (model.Prioritize != 0)
            {
                var existsPrioritize = await _context.Categories.AnyAsync(x => x.Prioritize == model.Prioritize);
                if (existsPrioritize)
                    throw new InvalidOperationException($"Prioritize {model.Prioritize} đã tồn tại");
            }

            var entity = new DBContext.BMWindows.Entities.Category
            {
                Name = model.Name,
                Status = model.Status == 0 ? (byte)1 : model.Status,
                Prioritize = model.Prioritize,
                Keyword = TextNormalizer.ToAsciiKeyword(model.Keyword ?? string.Empty),
                CreateBy = model.CreateBy,
                CreateTime = DateTime.UtcNow,
                UpdateBy = model.UpdateBy,
                UpdateTime = DateTime.UtcNow
            };

            await _context.Categories.AddAsync(entity);
            await _context.SaveChangesAsync();

            return new CategoryModel
            {
                Id = entity.Id,
                Name = entity.Name,
                Status = entity.Status,
                Prioritize = entity.Prioritize,
                Keyword = entity.Keyword,
                CreateBy = entity.CreateBy,
                CreateTime = entity.CreateTime,
                UpdateBy = entity.UpdateBy,
                UpdateTime = entity.UpdateTime
            };
        }

        public async Task<CategoryModel> UpdateCategory(CategoryModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (model.Id <= 0) throw new ArgumentOutOfRangeException(nameof(model), "Lỗi khi sửa danh mục");
            if (string.IsNullOrWhiteSpace(model.Name)) throw new ArgumentException("Tên danh mục không hợp lệ", nameof(model));
            if (SqlGuard.IsSuspicious(model)) throw new Exception("Đầu vào không hợp lệ");

            var entity = await _context.Categories.FirstOrDefaultAsync(x => x.Id == model.Id);
            if (entity == null)
                throw new KeyNotFoundException($"Không tìm thấy danh mục với id = {model.Id}");

            if (entity.Prioritize != model.Prioritize && model.Prioritize != 0)
            {
                var existsPrioritize = await _context.Categories.AnyAsync(x => x.Prioritize == model.Prioritize && x.Id != model.Id);
                if (existsPrioritize)
                    throw new InvalidOperationException($"Prioritize {model.Prioritize} đã tồn tại");
            }

            entity.Name = model.Name;
            entity.Status = model.Status;
            entity.Prioritize = model.Prioritize;
            entity.Keyword = TextNormalizer.ToAsciiKeyword(model.Keyword ?? string.Empty);
            entity.UpdateBy = model.UpdateBy;
            entity.UpdateTime = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new CategoryModel
            {
                Id = entity.Id,
                Status = entity.Status,
                Name = entity.Name,
                CreateBy = entity.CreateBy,
                CreateTime = entity.CreateTime,
                UpdateBy = entity.UpdateBy,
                UpdateTime = entity.UpdateTime,
                Prioritize = entity.Prioritize,
                Keyword = entity.Keyword
            };
        }

        public async Task<bool> ChangeStatus(int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id), "Lỗi khi đổi trạng thái");

            var entity = await _context.Categories.FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                throw new KeyNotFoundException($"Không tìm thấy danh mục với id = {id}");

            entity.Status = 0;
            entity.UpdateTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}