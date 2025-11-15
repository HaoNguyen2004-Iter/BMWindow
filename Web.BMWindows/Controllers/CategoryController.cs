using Microsoft.AspNetCore.Mvc;
using Service.BMWindows.Executes.AppItem;
using Service.BMWindows.Executes.Category;

namespace Web.BMWindows.Controllers
{
    public class CategoryController : Controller
    {
        private readonly CategoryMany _categoryMany;
        private readonly CategoryCommand _categoryCommand;
        private readonly CategoryOne _categoryOne;

        public CategoryController(CategoryMany categoryMany, CategoryCommand categoryCommand, CategoryOne categoryOne)
        {
            _categoryMany = categoryMany;
            _categoryCommand = categoryCommand;
            _categoryOne = categoryOne;
        }

        [HttpGet]
        public async Task<IActionResult> AppItemGroup([FromQuery] CategoryModel? filter, int page = 1, int pageSize = 5, string? part = null)
        {
            var data = await _categoryMany.GetAllCategory(page, pageSize, filter);
            // Path matches new structure: Views/Admin/AppItemCategory/Index.cshtml
            return PartialView("~/Views/Admin/AppItemCategory/Index.cshtml", data);
        }

        [HttpPost]
        public async Task<IActionResult> Update([FromForm] CategoryModel model)
        {
            try
            {
                if (model == null || model.Id <= 0)
                    return BadRequest(new { ok = false, message = "Dữ liệu không hợp lệ" });

                var updated = await _categoryCommand.UpdateCategory(model);
                return Ok(new { ok = true, data = updated, message = "Cập nhật thành công" });
            }
            catch (System.Exception ex)
            {
                Response.StatusCode = 400;
                return Json(new { ok = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CategoryModel model)
        {
            try
            {
                if (model == null)
                    return BadRequest(new { ok = false, message = "Dữ liệu không hợp lệ" });

                var created = await _categoryCommand.AddCategory(model);
                return Ok(new { ok = true, data = created, message = "Tạo thành công" });
            }
            catch (Exception ex)
            {
                Response.StatusCode = 400;
                return Json(new { ok = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int? id)
        {
            try
            {
                // Fixed path to include Admin segment per new folder structure.
                const string formViewPath = "~/Views/Admin/AppItemCategory/Form.cshtml";

                if (!id.HasValue || id.Value <= 0)
                {
                    var empty = new CategoryModel { Id = 0, Status = 1 };
                    return PartialView(formViewPath, empty);
                }

                var model = await _categoryOne.GetOneCategory(id.Value);
                return PartialView(formViewPath, model);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Content($"<div class='text-danger p-3'>Lỗi: {ex.Message}</div>", "text/html");
            }
        }
    }
}