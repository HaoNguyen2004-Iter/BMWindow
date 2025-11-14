using Microsoft.AspNetCore.Mvc;
using Service.BMWindows.Executes.AppItem;
using Service.BMWindows.Executes.Category;
using Service.BMWindows.Variables;

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
        public async Task<IActionResult> AppItemGroup()
        {
            var filter = new CategoryModel();
            var option = new OptionResult
            {
                Unlimited = true,
                OrderBy = nameof(CategoryModel.Prioritize),
                OrderType = "asc"
            };
            var data = await _categoryMany.GetAllCategory(filter, option);
            return PartialView("~/Views/AppItem/_AppItemGroup.cshtml", data.Many);
        }

        [HttpGet]
        public async Task<IActionResult> GetList([FromQuery] CategoryModel filter, [FromQuery] OptionResult option)
        {
            try
            {
                // Set default values nếu null
                if (option == null)
                {
                    option = new OptionResult
                    {
                        Page = 1,
                        Limit = 20,
                        HasCount = true,
                        OrderBy = nameof(CategoryModel.Prioritize),
                        OrderType = "asc"
                    };
                }
                else
                {
                    option.OrderBy ??= nameof(CategoryModel.Prioritize);
                    option.OrderType ??= "asc";
                }

                if (filter == null)
                {
                    filter = new CategoryModel();
                }

                var data = await _categoryMany.GetAllCategory(filter, option);
                
                return Json(new
                {
                    ok = true,
                    count = data.Count,
                    many = data.Many,
                    skip = data.Skip,
                    take = data.Take
                });
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { ok = false, message = ex.Message });
            }
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
        public async Task<IActionResult> ChangeStatus(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { ok = false, message = "Dữ liệu không hợp lệ" });
                var ok = await _categoryCommand.ChangeStatus(id);
                return Ok(new { ok });
            }
            catch (System.Exception ex)
            {
                Response.StatusCode = 400;
                return Json(new { ok = false, message = ex.Message });
            }
        }

        // NEW: create
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
                if (!id.HasValue || id.Value <= 0)
                {
                    // Create mode: empty model
                    var empty = new CategoryModel { Id = 0, Status = 1 };
                    return PartialView("~/Views/AppItem/_AppItemGroupView.cshtml", empty);
                }

                var model = await _categoryOne.GetOneCategory(id.Value);
                return PartialView("~/Views/AppItem/_AppItemGroupView.cshtml", model);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Content($"<div class='text-danger p-3'>Lỗi: {ex.Message}</div>", "text/html");
            }
        }
    }
}