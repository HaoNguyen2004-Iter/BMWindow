using Microsoft.EntityFrameworkCore;

namespace Service.BMWindows.Executes.AppItem
{
    public class AppItemOne
    {
        private readonly DBContext.BMWindows.Entities.BMWindowDBContext _context;

        public AppItemOne(DBContext.BMWindows.Entities.BMWindowDBContext context)
        {
            _context = context;
        }

        public async Task<AppItemModel> GetOneAppItem(int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException("Lỗi khi tìm ứng dụng");

            var result = await _context.AppItems
                .Where(x => x.Id == id)
                .Select(x => new AppItemModel
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
                })
                .FirstOrDefaultAsync();

            if (result == null)
                throw new KeyNotFoundException($"Không tìm thấy AppItem với id = {id}");

            return result;
        }
    }
}