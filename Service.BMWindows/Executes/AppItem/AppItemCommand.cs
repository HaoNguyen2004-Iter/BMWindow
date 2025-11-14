//using Microsoft.EntityFrameworkCore;
//using SPMH.Services.Utils;

//namespace Service.BMWindows.Executes.AppItem
//{
//    public class AppItemCommand
//    {
//        private readonly DBContext.BMWindows.Entities.BMWindowDBContext _context;

//        public AppItemCommand(DBContext.BMWindows.Entities.BMWindowDBContext context)
//        {
//            _context = context;
//        }

//        public async Task<AppItemModel> AddAppItem(AppItemModel model)
//        {
//            if (model == null) throw new ArgumentNullException(nameof(model));
//            if (string.IsNullOrWhiteSpace(model.Name)) throw new ArgumentException("Tên không hợp lệ", nameof(model));
//            if (model.CategoryId <= 0) throw new ArgumentOutOfRangeException(nameof(model.CategoryId), "CategoryId không hợp lệ");
//            if (SqlGuard.IsSuspicious(model)) throw new Exception("Đầu vào không hợp lệ");

//            var existsPrioritize = await _context.AppItems.AnyAsync(x => x.Prioritize == model.Prioritize);
//            if (existsPrioritize)
//                throw new InvalidOperationException($"Prioritize {model.Prioritize} đã tồn tại");

//            var now = DateTime.UtcNow;
//            var entity = new DBContext.BMWindows.Entities.AppItem
//            {
//                CategoryId = model.CategoryId,
//                Name = model.Name,
//                Icon = model.Icon,
//                Size = model.Size,
//                Url = model.Url,
//                Prioritize = model.Prioritize,
//                Status = model.Status == 0 ? 1 : model.Status,
//                Keyword = TextNormalizer.ToAsciiKeyword(model.Keyword ?? string.Empty),
//                CreatedBy = model.CreatedBy,      // map model -> entity
//                CreatedDate = now,
//                UpdatedBy = model.UpdatedBy == Guid.Empty ? model.CreatedBy : model.UpdatedBy,
//                UpdatedDate = now
//            };

//            await _context.AppItems.AddAsync(entity);
//            await _context.SaveChangesAsync();

//            return new AppItemModel
//            {
//                Id = entity.Id,
//                CategoryId = entity.CategoryId,
//                Name = entity.Name,
//                Icon = entity.Icon,
//                Size = entity.Size,
//                Url = entity.Url,
//                Prioritize = entity.Prioritize,
//                Status = entity.Status,
//                Keyword = entity.Keyword,
//                CreatedBy = entity.CreatedBy,     
//                CreatedDate = entity.CreatedDate,
//                UpdatedBy = entity.UpdatedBy,
//                UpdatedDate = entity.UpdatedDate
//            };
//        }

//        public async Task<AppItemModel> UpdateAppItem(AppItemModel model)
//        {
//            if (model == null) throw new ArgumentNullException(nameof(model));
//            if (model.Id <= 0) throw new ArgumentOutOfRangeException(nameof(model.Id), "Id không hợp lệ");
//            if (string.IsNullOrWhiteSpace(model.Name)) throw new ArgumentException("Tên không hợp lệ", nameof(model));
//            if (SqlGuard.IsSuspicious(model)) throw new Exception("Đầu vào không hợp lệ");

//            var entity = await _context.AppItems.FirstOrDefaultAsync(x => x.Id == model.Id);
//            if (entity == null)
//                throw new KeyNotFoundException($"Không tìm thấy AppItem với id = {model.Id}");

//            if (entity.Prioritize != model.Prioritize)
//            {
//                var existsPrioritize = await _context.AppItems.AnyAsync(x => x.Prioritize == model.Prioritize && x.Id != model.Id);
//                if (existsPrioritize)
//                    throw new InvalidOperationException($"Prioritize {model.Prioritize} đã tồn tại");
//            }

//            entity.CategoryId = model.CategoryId;
//            entity.Name = model.Name;
//            entity.Icon = model.Icon;
//            entity.Size = model.Size;
//            entity.Url = model.Url;
//            entity.Prioritize = model.Prioritize;
//            entity.Status = model.Status;
//            entity.Keyword = TextNormalizer.ToAsciiKeyword(model.Keyword ?? string.Empty);
//            entity.UpdatedBy = model.UpdatedBy == Guid.Empty ? entity.UpdatedBy : model.UpdatedBy;
//            entity.UpdatedDate = DateTime.UtcNow;

//            await _context.SaveChangesAsync();

//            return new AppItemModel
//            {
//                Id = entity.Id,
//                CategoryId = entity.CategoryId,
//                Name = entity.Name,
//                Icon = entity.Icon,
//                Size = entity.Size,
//                Url = entity.Url,
//                Prioritize = entity.Prioritize,
//                Status = entity.Status,
//                Keyword = entity.Keyword,
//                CreatedBy = entity.CreatedBy,
//                CreatedDate = entity.CreatedDate,
//                UpdatedBy = entity.UpdatedBy,
//                UpdatedDate = entity.UpdatedDate
//            };
//        }

//        public async Task<bool> ChangeStatus(int id)
//        {
//            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id), "Lỗi khi đổi trạng thái");

//            var entity = await _context.AppItems.FirstOrDefaultAsync(x => x.Id == id);
//            if (entity == null)
//                throw new KeyNotFoundException($"Không tìm thấy ứng dụng với id = {id}");

//            entity.Status = 0;
//            entity.UpdatedDate = DateTime.UtcNow;
//            await _context.SaveChangesAsync();
//            return true;
//        }
//    }
//}