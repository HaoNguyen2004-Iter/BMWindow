using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Service.BMWindows.Executes.AppItem
{
    public class AppItemOne
    {
        private readonly DBContext.BMWindows.Entities.BMWindowDBContext _context;

        public AppItemOne(DBContext.BMWindows.Entities.BMWindowDBContext context)
        {
            _context = context;
        }

        public async Task<AppItemModel> GetOneAsync(int id)
        {
            if (id <= 0)
                throw new System.ArgumentOutOfRangeException(nameof(id), "Id không hợp lệ");

            // LEFT JOIN lấy CategoryName
            var item = await _context.AppItems
                .Where(x => x.Id == id)
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
                .FirstOrDefaultAsync();

            if (item == null)
                throw new System.Collections.Generic.KeyNotFoundException($"Không tìm thấy AppItem với id = {id}");

            return item;
        }
    }
}