﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Repository;

namespace Shopping_Tutorial.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin, Author")]
public class CategoryController : Controller
{
    private readonly DataContext _dataContext;
    public CategoryController(DataContext context)
    {
        _dataContext = context;
    }


    //[Route("Index")]
    public async Task<IActionResult> Index(int pg = 1)
    {
        List<CategoryModel> category = _dataContext.Categories.ToList(); //33 datas


        const int pageSize = 10; //10 items/trang

        if (pg < 1) //page < 1;
        {
            pg = 1; //page ==1
        }
        int recsCount = category.Count(); //33 items;

        var pager = new Paginate(recsCount, pg, pageSize);

        int recSkip = (pg - 1) * pageSize; //(3 - 1) * 10; 

        //category.Skip(20).Take(10).ToList()

        var data = category.Skip(recSkip).Take(pager.PageSize).ToList();

        ViewBag.Pager = pager;

        return View(data);
    }

    //public async Task<IActionResult> Index()
    //{
    //    return View(await _dataContext.Categories.OrderByDescending(p => p.Id).ToArrayAsync());
    //}

    public async Task<IActionResult> Edit(int Id)
    {
        CategoryModel category = await _dataContext.Categories.FindAsync(Id);
        return View(category);
    }

    public async Task<IActionResult> Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryModel category)
    {
        if (ModelState.IsValid)
        {
            category.Slug = category.Name.Replace(" ", "-");
            var slug = await _dataContext.Categories.FirstOrDefaultAsync(p => p.Slug == category.Slug);
            if (slug != null)
            {
                ModelState.AddModelError("", "Danh mục đã có trong database");
                return View(category);
            }
            _dataContext.Add(category);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Thêm danh mục thành công";
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
        return View(category);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CategoryModel category)
    {
        if (ModelState.IsValid)
        {
            category.Slug = category.Name.Replace(" ", "-");
            var slug = await _dataContext.Categories.FirstOrDefaultAsync(p => p.Slug == category.Slug);
            if (slug != null)
            {
                ModelState.AddModelError("", "Danh mục đã có trong database");
                return View(category);
            }
            _dataContext.Update(category);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Sửa danh mục thành công";
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
        return View(category);
    }

    public async Task<IActionResult> Delete(int Id)
    {
        CategoryModel category = await _dataContext.Categories.FindAsync(Id);
        _dataContext.Categories.Remove(category);
        await _dataContext.SaveChangesAsync();
        TempData["success"] = "Danh mục đã xóa thành công";
        return RedirectToAction("Index");
    }
    

}