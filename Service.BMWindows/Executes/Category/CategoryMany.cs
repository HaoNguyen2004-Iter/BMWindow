using Microsoft.EntityFrameworkCore;
using SPMH.Services.Utils;

namespace Service.BMWindows.Executes.Category
{
    public class CategoryMany
    {
        private readonly DBContext.BMWindows.Entities.BMWindowDBContext _context;

        public CategoryMany(DBContext.BMWindows.Entities.BMWindowDBContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<CategoryModel>> GetAllCategory(int page, int pageSize, CategoryModel? filter)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 5;

            if (filter != null && SqlGuard.IsSuspicious(filter))
                throw new ArgumentException("Đầu vào không hợp lệ");

            var query = BuildQuery(filter);

            var ordered = query
                .OrderBy(c => c.Prioritize)
                .ThenByDescending(c => c.Id);

            var total = await query.CountAsync();
            var items = await ordered
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
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
                .ToListAsync();

            return new PagedResult<CategoryModel>(items, total, page, pageSize);
        }

        private IQueryable<DBContext.BMWindows.Entities.Category> BuildQuery(CategoryModel? model)
        {
            var q = _context.Categories.AsQueryable();
            if (model == null) return q;

            if (model.Id != 0)
                q = q.Where(x => x.Id == model.Id);

            if (!string.IsNullOrWhiteSpace(model.Keyword))
            {
                var kw = TextNormalizer.ToAsciiKeyword(model.Keyword);
                q = q.Where(x => x.Keyword!.Contains(kw));
            }

            if (model.CreateDateFrom != null)
                q = q.Where(x => x.CreatedDate >= model.CreateDateFrom.Value);

            if (model.CreateDateTo != null)
            {
                var toCreate = model.CreateDateTo.Value.Date.AddDays(1).AddMilliseconds(-1);
                q = q.Where(x => x.CreatedDate <= toCreate);
            }

            if (model.UpdateDateFrom != null)
                q = q.Where(x => x.UpdatedDate >= model.UpdateDateFrom.Value);

            if (model.UpdateDateTo != null)
            {
                var toUpdate = model.UpdateDateTo.Value.Date.AddDays(1).AddMilliseconds(-1);
                q = q.Where(x => x.UpdatedDate <= toUpdate);
            }

            return q;
        }
    }
}