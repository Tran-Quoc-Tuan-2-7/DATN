﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Tutorial.Repository;

namespace Shopping_Tutorial.Areas.Admin.Controllers;

[Area("Admin")]
public class CategoryController : Controller
{
    private readonly DataContext _dataContext;
    public CategoryController(DataContext context)  
    {
        _dataContext = context;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _dataContext.Categories.OrderByDescending(p => p.Id).ToArrayAsync());
    }
}
