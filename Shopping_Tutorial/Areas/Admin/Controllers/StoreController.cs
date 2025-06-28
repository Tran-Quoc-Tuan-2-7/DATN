using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Repository;

namespace Shopping_Tutorial.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]

public class StoreController : Controller
{
    private readonly DataContext _dataContext;
    private readonly IWebHostEnvironment _webHostEnvironment;
    public StoreController(DataContext context, IWebHostEnvironment webHostEnvironment)
    {
        _dataContext = context;
        _webHostEnvironment = webHostEnvironment;
    }
    public IActionResult Index()
    {
        var store = _dataContext.Stores.ToList();
        return View(store);
    }

    public async Task<IActionResult> Edit(int Id)
    {
        var store = await _dataContext.Stores.FindAsync(Id);
        if (store == null)
        {
            return NotFound();
        }
        return View(store);
    }


    public async Task<IActionResult> Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(StoreModel Store)
    {
        if (ModelState.IsValid)
        {
            _dataContext.Add(Store);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Thêm cửa hàng thành công";
            return RedirectToAction("Index");
        }

        else
        {
            TempData["error"] = "Có một vài thứ đang bị lỗi";
            List<string> errors = new List<string>();
            foreach (var value in ModelState.Values)
            {
                foreach (var error in value.Errors)
                {
                    errors.Add(error.ErrorMessage);
                }
            }
            string errorMessage = string.Join("\n", errors);
            return BadRequest(errorMessage);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(StoreModel Store)
    {
        if (ModelState.IsValid)
        {
            _dataContext.Update(Store);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Sửa cửa hàng thành công";
            return RedirectToAction("Index");
        }

        else
        {
            TempData["error"] = "Có một vài thứ đang bị lỗi";
            List<string> errors = new List<string>();
            foreach (var value in ModelState.Values)
            {
                foreach (var error in value.Errors)
                {
                    errors.Add(error.ErrorMessage);
                }
            }
            string errorMessage = string.Join("\n", errors);
            return BadRequest(errorMessage);
        }
    }

    public async Task<IActionResult> Delete(int Id)
    {
        StoreModel Store = await _dataContext.Stores.FindAsync(Id);
        _dataContext.Stores.Remove(Store);
        await _dataContext.SaveChangesAsync();
        TempData["success"] = "Cửa hàng đã xóa thành công";
        return RedirectToAction("Index");
    }
}
