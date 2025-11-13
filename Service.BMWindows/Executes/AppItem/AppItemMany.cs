using Service.BMWindows.Variables;
using SPMH.Services.Utils;

namespace Service.BMWindows.Executes.AppItem
{
    public class AppItemMany
    {
        private readonly DBContext.BMWindows.Entities.BMWindowDBContext _context;

        public AppItemMany(DBContext.BMWindows.Entities.BMWindowDBContext context)
        {
            _context = context;
        }

        public Task<QueryResult<AppItemModel>> GetAllAppItem(AppItemModel model, OptionResult option)
            => AppItemData(model, option);

        private async Task<QueryResult<AppItemModel>> AppItemData(AppItemModel model, OptionResult option)
        {
            if (SqlGuard.IsSuspicious(model))
                throw new Exception("Đầu vào không hợp lệ");

            IQueryable<DBContext.BMWindows.Entities.AppItem> q = _context.AppItems;

            if (model.Id != 0)
                q = q.Where(x => x.Id == model.Id);

            if (model.CategoryId != 0)
                q = q.Where(x => x.CategoryId == model.CategoryId);

            if (!string.IsNullOrWhiteSpace(model.Keyword))
            {
                var kw = TextNormalizer.ToAsciiKeyword(model.Keyword);
                q = q.Where(x => x.Keyword.Contains(kw));
            }

            if (model.CreateDateFrom != null)
                q = q.Where(x => x.CreateTime >= model.CreateDateFrom.Value);
            if (model.CreateDateTo != null)
            {
                var toCreate = model.CreateDateTo.Value.Date.AddDays(1).AddMilliseconds(-1);
                q = q.Where(x => x.CreateTime <= toCreate);
            }

            if (model.UpdateDateFrom != null)
                q = q.Where(x => x.UpdateTime >= model.UpdateDateFrom.Value);
            if (model.UpdateDateTo != null)
            {
                var toUpdate = model.UpdateDateTo.Value.Date.AddDays(1).AddMilliseconds(-1);
                q = q.Where(x => x.UpdateTime <= toUpdate);
            }

            if (model.Prioritize != 0)
                q = q.Where(x => x.Prioritize == model.Prioritize);

            if (model.Status != 0)
                q = q.Where(x => x.Status == model.Status);

            var result = q.Select(x => new AppItemModel
            {
                Id = x.Id,
                CategoryId = x.CategoryId,
                Name = x.Name,
                Icon = x.Icon,
                Size = x.Size,
                Url = x.Url,
                Token = x.Token,
                Expired = x.Expired,
                AppExpire = x.AppExpire,
                Prioritize = x.Prioritize,
                CreateBy = x.CreateBy,
                CreateTime = x.CreateTime,
                Status = x.Status,
                Keyword = x.Keyword,
                UpdateBy = x.UpdateBy,
                UpdateTime = x.UpdateTime
            });

            return await Task.FromResult(new QueryResult<AppItemModel>(result, option));
        }
    }
}