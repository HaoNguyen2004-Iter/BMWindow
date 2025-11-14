//using Microsoft.EntityFrameworkCore;
//using SPMH.Services.Utils;

//namespace Service.BMWindows.Executes.AppItem
//{
//    public class AppItemMany
//    {
//        private readonly DBContext.BMWindows.Entities.BMWindowDBContext _context;

//        public AppItemMany(DBContext.BMWindows.Entities.BMWindowDBContext context)
//        {
//            _context = context;
//        }

//        public async Task<PagedResult<AppItemModel>> GetAllAppItem(int page, int pageSize, AppItemModel? filter)
//        {
//            if (page < 1) page = 1;
//            if (pageSize < 1) pageSize = 5;

//            if (filter != null && SqlGuard.IsSuspicious(filter))
//                throw new ArgumentException("Đầu vào không hợp lệ");

//            var query = BuildQuery(filter);

//            var ordered = query
//                .OrderBy(x => x.Prioritize)
//                .ThenByDescending(x => x.Id);

//            var total = await query.CountAsync();

//            var items = await ordered
//                .Skip((page - 1) * pageSize)
//                .Take(pageSize)
//                .Select(x => new AppItemModel
//                {
//                    Id = x.Id,
//                    CategoryId = x.CategoryId,
//                    Name = x.Name,
//                    Icon = x.Icon,
//                    Size = x.Size,
//                    Url = x.Url,
//                    Prioritize = x.Prioritize,
//                    Status = x.Status,
//                    Keyword = x.Keyword,
//                    CreatedBy = x.CreatedBy,
//                    CreatedDate = x.CreatedDate,
//                    UpdatedBy = x.UpdatedBy,
//                    UpdatedDate = x.UpdatedDate
//                })
//                .ToListAsync();

//            return new PagedResult<AppItemModel>(items, total, page, pageSize);
//        }

//        private IQueryable<DBContext.BMWindows.Entities.AppItem> BuildQuery(AppItemModel? model)
//        {
//            var q = _context.AppItems.AsQueryable();
//            if (model == null) return q;

//            if (model.Id != 0)
//                q = q.Where(x => x.Id == model.Id);

//            if (model.CategoryId != 0)
//                q = q.Where(x => x.CategoryId == model.CategoryId);

//            if (!string.IsNullOrWhiteSpace(model.Keyword))
//            {
//                var kw = TextNormalizer.ToAsciiKeyword(model.Keyword);
//                q = q.Where(x => x.Keyword!.Contains(kw));
//            }

//            if (model.CreateDateFrom != null)
//                q = q.Where(x => x.CreatedDate >= model.CreateDateFrom.Value);
//            if (model.CreateDateTo != null)
//            {
//                var toCreate = model.CreateDateTo.Value.Date.AddDays(1).AddMilliseconds(-1);
//                q = q.Where(x => x.CreatedDate <= toCreate);
//            }

//            if (model.UpdateDateFrom != null)
//                q = q.Where(x => x.UpdatedDate >= model.UpdateDateFrom.Value);
//            if (model.UpdateDateTo != null)
//            {
//                var toUpdate = model.UpdateDateTo.Value.Date.AddDays(1).AddMilliseconds(-1);
//                q = q.Where(x => x.UpdatedDate <= toUpdate);
//            }

//            if (model.Prioritize != 0)
//                q = q.Where(x => x.Prioritize == model.Prioritize);

//            if (model.Status != 0)
//                q = q.Where(x => x.Status == model.Status);

//            return q;
//        }
//    }
//}