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

    public async Task<IActionResult> Edit()
    {
        StoreModel store = await _dataContext.Stores.FirstOrDefaultAsync();
        return View(store);
    }
}
