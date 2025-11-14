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

            var now = DateTime.UtcNow;
            var entity = new DBContext.BMWindows.Entities.Category
            {
                Name = model.Name,
                Status = model.Status == 0 ? 1 : model.Status,
                Keyword = TextNormalizer.ToAsciiKeyword(model.Keyword ?? string.Empty),
                CreatedBy = model.CreatedBy,
                CreatedDate = now,
                UpdatedBy = model.UpdatedBy,
                UpdatedDate = now
            };

            await _context.Categories.AddAsync(entity);
            await _context.SaveChangesAsync();

            return new CategoryModel
            {
                Id = entity.Id,
                Name = entity.Name,
                Status = entity.Status,
                Keyword = entity.Keyword,
                CreatedBy = entity.CreatedBy,
                CreatedDate = entity.CreatedDate,
                UpdatedBy = entity.UpdatedBy ?? Guid.Empty,
                UpdatedDate = entity.UpdatedDate
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

            entity.Name = model.Name;
            entity.Status = model.Status;
            entity.Keyword = TextNormalizer.ToAsciiKeyword(model.Keyword ?? string.Empty);
            entity.UpdatedBy = model.UpdatedBy;
            entity.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new CategoryModel
            {
                Id = entity.Id,
                Status = entity.Status,
                Name = entity.Name,
                CreatedBy = entity.CreatedBy,
                CreatedDate = entity.CreatedDate,
                UpdatedBy = entity.UpdatedBy ?? Guid.Empty,
                UpdatedDate = entity.UpdatedDate,
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
            entity.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}