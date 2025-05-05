using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Repository;

namespace Shopping_Tutorial.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin, Author")]
public class BrandController : Controller
{
    private readonly DataContext _dataContext;
    public BrandController(DataContext context)
    {
        _dataContext = context;
    }

    [Route("Index")]
    public async Task<IActionResult> Index(int pg = 1)
    {
        List<BrandModel> brand = _dataContext.Brands.ToList(); //33 datas


        const int pageSize = 10; //10 items/trang

        if (pg < 1) //page < 1;
        {
            pg = 1; //page ==1
        }
        int recsCount = brand.Count(); //33 items;

        var pager = new Paginate(recsCount, pg, pageSize);

        int recSkip = (pg - 1) * pageSize; //(3 - 1) * 10; 

        //Brand.Skip(20).Take(10).ToList()

        var data = brand.Skip(recSkip).Take(pager.PageSize).ToList();

        ViewBag.Pager = pager;

        return View(data);
    }

    //public async Task<IActionResult> Index()
    //{
    //    return View(await _dataContext.Brands.OrderByDescending(p => p.Id).ToArrayAsync());
    //}

    public async Task<IActionResult> Edit(int Id)
    {
        BrandModel brand = await _dataContext.Brands.FindAsync(Id);
        return View(brand);
    }

    public async Task<IActionResult> Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BrandModel Brand)
    {
        if (ModelState.IsValid)
        {
            Brand.Slug = Brand.Name.Replace(" ", "-");
            var slug = await _dataContext.Brands.FirstOrDefaultAsync(p => p.Slug == Brand.Slug);
            if (slug != null)
            {
                ModelState.AddModelError("", "Thương hiệu đã có trong database");
                return View(Brand);
            }
            _dataContext.Add(Brand);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Thêm Thương hiệu thành công";
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
    public async Task<IActionResult> Edit(BrandModel Brand)
    {
        if (ModelState.IsValid)
        {
            Brand.Slug = Brand.Name.Replace(" ", "-");
            var slug = await _dataContext.Brands.FirstOrDefaultAsync(p => p.Slug == Brand.Slug);
            if (slug != null)
            {
                ModelState.AddModelError("", "Thương hiệu đã có trong database");
                return View(Brand);
            }
            _dataContext.Update(Brand);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Sửa Thương hiệu thành công";
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
        BrandModel brand = await _dataContext.Brands.FindAsync(Id);
        _dataContext.Brands.Remove(brand);
        await _dataContext.SaveChangesAsync();
        TempData["success"] = "Thương hiệu đã xóa thành công";
        return RedirectToAction("Index");
    }


}