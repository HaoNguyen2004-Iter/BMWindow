using Service.BMWindows.Variables;
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

        public Task<QueryResult<CategoryModel>> GetAllCategory(CategoryModel model, OptionResult option)
        {
            return CategoryData(model, option);
        }

        private async Task<QueryResult<CategoryModel>> CategoryData(CategoryModel model, OptionResult option)
        {
            if (SqlGuard.IsSuspicious(model))
                throw new Exception("Đầu vào không hợp lệ");

            IQueryable<DBContext.BMWindows.Entities.Category> q = _context.Categories.AsQueryable();

            if (model.Id != 0)
                q = q.Where(x => x.Id == model.Id);

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

            var projected = q.Select(x => new CategoryModel
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
            });

            return await Task.FromResult(new QueryResult<CategoryModel>(projected, option));
        }
    }
}