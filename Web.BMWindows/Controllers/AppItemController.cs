using Microsoft.AspNetCore.Mvc;
using Service.BMWindows.Executes.AppItem;
using Service.BMWindows.Variables;

namespace BMWindows.Controllers
{
    public class AppItemController : Controller
    {
        private readonly AppItemMany _appItemMany;
        private readonly AppItemCommand _appItemCommand;

        public AppItemController(AppItemMany appItemMany, AppItemCommand appItemCommand)
        {
            _appItemMany = appItemMany;
            _appItemCommand = appItemCommand;
        }

        [HttpGet]
        public async Task<IActionResult> AppItemList()
        {
            var filter = new AppItemModel();
            var option = new OptionResult
            {
                Unlimited = true,
                OrderBy = nameof(AppItemModel.Prioritize),
                OrderType = "asc"
            };
            var data = await _appItemMany.GetAllAppItem(filter, option);
            return PartialView("~/Views/AppItem/_AppItemList.cshtml", data.Many);
        }

        [HttpPost]
        public async Task<IActionResult> Update([FromForm] AppItemModel model)
        {
            try
            {
                if (model == null || model.Id <= 0)
                    return BadRequest(new { ok = false, message = "Dữ liệu không hợp lệ" });

                var updated = await _appItemCommand.UpdateAppItem(model);
                return Ok(new { ok = true, data = updated, message = "Cập nhật thành công" });
            }
            catch (System.Exception ex)
            {
                Response.StatusCode = 400;
                return Json(new { ok = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ChangeStatus(int id)
        {
            try
            {
                var ok = await _appItemCommand.ChangeStatus(id);
                return Ok(new { ok });
            }
            catch (System.Exception ex)
            {
                Response.StatusCode = 400;
                return Json(new { ok = false, message = ex.Message });
            }
        }
    }
}
