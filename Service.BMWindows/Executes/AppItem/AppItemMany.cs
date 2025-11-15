using Microsoft.EntityFrameworkCore;
using SPMH.Services.Utils;
using System.Linq;
using System.Threading.Tasks;

namespace Service.BMWindows.Executes.AppItem
{
    public class AppItemMany
    {
        private readonly DBContext.BMWindows.Entities.BMWindowDBContext _context;

        public AppItemMany(DBContext.BMWindows.Entities.BMWindowDBContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<AppItemModel>> GetPagedAsync(int page, int pageSize, AppItemModel? filter)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            if (filter != null && SqlGuard.IsSuspicious(filter))
                throw new System.ArgumentException("Đầu vào không hợp lệ");

            var query = BuildQuery(filter);

            var ordered = query
                .OrderBy(x => x.Prioritize)
                .ThenByDescending(x => x.Id);

            var total = await query.CountAsync();

            var items = await ordered
                .GroupJoin(_context.Categories,
                    app => app.CategoryId,
                    cat => cat.Id,
                    (app, cats) => new { app, cats })
                .SelectMany(x => x.cats.DefaultIfEmpty(),
                    (x, cat) => new AppItemModel
                    {
                        Id = x.app.Id,
                        CategoryId = x.app.CategoryId,
                        CategoryName = cat != null ? cat.Name : null,
                        Name = x.app.Name,
                        Icon = x.app.Icon,
                        Size = x.app.Size,
                        Url = x.app.Url,
                        Prioritize = x.app.Prioritize,
                        Status = x.app.Status,
                        Keyword = x.app.Keyword,
                        CreatedBy = x.app.CreatedBy,
                        CreatedDate = x.app.CreatedDate,
                        UpdatedBy = x.app.UpdatedBy,
                        UpdatedDate = x.app.UpdatedDate
                    })
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<AppItemModel>(items, total, page, pageSize);
        }

        private IQueryable<DBContext.BMWindows.Entities.AppItem> BuildQuery(AppItemModel? model)
        {
            var q = _context.AppItems.AsQueryable();

            if (model == null) return q;

            if (model.Id > 0)
                q = q.Where(x => x.Id == model.Id);

            if (model.CategoryId > 0)
                q = q.Where(x => x.CategoryId == model.CategoryId);

            if (!string.IsNullOrWhiteSpace(model.Keyword))
            {
                var kw = TextNormalizer.ToAsciiKeyword(model.Keyword);
                q = q.Where(x => x.Keyword != null && x.Keyword.Contains(kw));
            }

            if (model.CreateDateFrom != null)
                q = q.Where(x => x.CreatedDate >= model.CreateDateFrom.Value);

            if (model.CreateDateTo != null)
            {
                var to = model.CreateDateTo.Value.Date.AddDays(1).AddMilliseconds(-1);
                q = q.Where(x => x.CreatedDate <= to);
            }

            if (model.UpdateDateFrom != null)
                q = q.Where(x => x.UpdatedDate >= model.UpdateDateFrom.Value);

            if (model.UpdateDateTo != null)
            {
                var to = model.UpdateDateTo.Value.Date.AddDays(1).AddMilliseconds(-1);
                q = q.Where(x => x.UpdatedDate <= to);
            }

            if (model.Prioritize != 0)
                q = q.Where(x => x.Prioritize == model.Prioritize);

            if (model.Status == 0 || model.Status == 1)
                q = q.Where(x => x.Status == model.Status);

            return q;
        }
    }
}