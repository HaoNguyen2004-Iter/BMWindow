using Microsoft.EntityFrameworkCore;
using SPMH.Services.Utils;

namespace Service.BMWindows.Executes.AppItem
{
    using DB = DBContext.BMWindows.Entities;

    public class AppItemCommand
    {
        private readonly DB.BMWindowDBContext _context;

        public AppItemCommand(DB.BMWindowDBContext context)
        {
            _context = context;
        }

        private async Task<(int Id, string Name)> EnsureCategoryAsync(int categoryId)
        {
            if (categoryId <= 0)
                throw new ArgumentOutOfRangeException(nameof(categoryId), "CategoryId không hợp lệ");

            var cat = await _context.Categories
                .Where(x => x.Id == categoryId && x.Status == 1)
                .Select(x => new { x.Id, x.Name })
                .FirstOrDefaultAsync();

            if (cat == null)
                throw new KeyNotFoundException($"CategoryId={categoryId} không tồn tại hoặc không hoạt động");

            return (cat.Id, cat.Name);
        }

        public async Task<AppItemModel> AddAppItem(AppItemModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (string.IsNullOrWhiteSpace(model.Name)) throw new ArgumentException("Tên không hợp lệ", nameof(model));
            if (SqlGuard.IsSuspicious(model)) throw new Exception("Đầu vào không hợp lệ");

            var category = await EnsureCategoryAsync(model.CategoryId);

            var existsPrioritize = await _context.AppItems.AnyAsync(x => x.Prioritize == model.Prioritize);
            if (existsPrioritize)
                throw new InvalidOperationException($"Prioritize {model.Prioritize} đã tồn tại");

            var now = DateTime.UtcNow;
            var entity = new DB.AppItem
            {
                CategoryId = category.Id,
                Name = model.Name.Trim(),
                Icon = model.Icon,
                Size = model.Size,
                Url = model.Url,
                Prioritize = model.Prioritize,
                Status = model.Status == 0 ? 1 : model.Status,
                Keyword = TextNormalizer.ToAsciiKeyword(model.Keyword ?? model.Name),
                CreatedBy = model.CreatedBy,
                CreatedDate = now,
                UpdatedBy = model.UpdatedBy == Guid.Empty ? model.CreatedBy : model.UpdatedBy,
                UpdatedDate = now
            };

            await _context.AppItems.AddAsync(entity);
            await _context.SaveChangesAsync();

            var categoryName = category.Name;
            return Map(entity, categoryName);
        }

        public async Task<AppItemModel> UpdateAppItem(AppItemModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (model.Id <= 0) throw new ArgumentOutOfRangeException(nameof(model.Id), "Id không hợp lệ");
            if (string.IsNullOrWhiteSpace(model.Name)) throw new ArgumentException("Tên không hợp lệ", nameof(model));
            if (SqlGuard.IsSuspicious(model)) throw new Exception("Đầu vào không hợp lệ");

            var entity = await _context.AppItems.FirstOrDefaultAsync(x => x.Id == model.Id);
            if (entity == null)
                throw new KeyNotFoundException($"Không tìm thấy AppItem với id = {model.Id}");

            string? categoryName = null;
            if (entity.CategoryId != model.CategoryId)
            {
                var category = await EnsureCategoryAsync(model.CategoryId);
                entity.CategoryId = category.Id;
                categoryName = category.Name;
            }

            if (entity.Prioritize != model.Prioritize)
            {
                var existsPrioritize = await _context.AppItems.AnyAsync(x => x.Prioritize == model.Prioritize && x.Id != model.Id);
                if (existsPrioritize)
                    throw new InvalidOperationException($"Prioritize {model.Prioritize} đã tồn tại");
            }

            entity.Name = model.Name.Trim();
            entity.Icon = model.Icon;
            entity.Size = model.Size;
            entity.Url = model.Url;
            entity.Prioritize = model.Prioritize;
            entity.Status = model.Status;
            entity.Keyword = TextNormalizer.ToAsciiKeyword(model.Keyword ?? model.Name);
            entity.UpdatedBy = model.UpdatedBy == Guid.Empty ? entity.UpdatedBy : model.UpdatedBy;
            entity.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            if (categoryName == null)
            {
                categoryName = await _context.Categories
                    .Where(c => c.Id == entity.CategoryId)
                    .Select(c => c.Name)
                    .FirstOrDefaultAsync();
            }

            return Map(entity, categoryName);
        }

        public async Task<bool> ChangeStatus(int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id), "Lỗi khi đổi trạng thái");

            var entity = await _context.AppItems.FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                throw new KeyNotFoundException($"Không tìm thấy ứng dụng với id = {id}");

            entity.Status = entity.Status == 1 ? 0 : 1;
            entity.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        private static AppItemModel Map(DB.AppItem x, string? categoryName) => new()
        {
            Id = x.Id,
            CategoryId = x.CategoryId,
            CategoryName = categoryName,
            Name = x.Name,
            Icon = x.Icon,
            Size = x.Size,
            Url = x.Url,
            Prioritize = x.Prioritize,
            Status = x.Status,
            Keyword = x.Keyword,
            CreatedBy = x.CreatedBy,
            CreatedDate = x.CreatedDate,
            UpdatedBy = x.UpdatedBy,
            UpdatedDate = x.UpdatedDate
        };
    }
}