using Microsoft.AspNetCore.Mvc;
using Shopping_Tutorial.Repository;
using Shopping_Tutorial.Models;
using Microsoft.EntityFrameworkCore;

namespace Shopping_Tutorial.Areas.Admin.Controllers;

[Area("Admin")]
public class DashboardController : Controller
{
    private readonly DataContext _dataContext;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public DashboardController(DataContext context, IWebHostEnvironment webHostEnvironment)
    {
        _dataContext = context;
        _webHostEnvironment = webHostEnvironment;
    }

    public IActionResult Index()
    {
        var count_product = _dataContext.Products.Count();
        var count_order = _dataContext.Orders.Count();
        var count_category = _dataContext.Categories.Count();
        var count_user = _dataContext.Users.Count();

        ViewBag.CountProduct = count_product;
        ViewBag.CountOrder = count_order;
        ViewBag.CountCategory = count_category;
        ViewBag.CountUser = count_user;

        // Truyền danh sách cửa hàng để hiển thị dropdown
        ViewBag.Stores = _dataContext.Stores.ToList();

        return View();
    }

    [HttpPost]
    public IActionResult GetChartData()
    {
        var data = _dataContext.Statisticals
            .OrderBy(s => s.DateCreated)
            .Select(s => new
            {
                date = s.DateCreated.ToString("dd-MM-yyyy"),
                sold = s.Sold,
                quantity = s.Quantity,
                revenue = s.Revenue,
                profit = s.Profit
            })
            .ToList();

        return Json(data);
    }

    [HttpPost]
    public IActionResult GetChartDataBySelect(DateTime startDate, DateTime endDate, int storeId = 0)
    {
        var query = _dataContext.Statisticals
            .Include(s => s.Store) // Nếu bạn có liên kết đến Store
            .Where(s => s.DateCreated.Date >= startDate.Date && s.DateCreated.Date <= endDate.Date);

        if (storeId != 0)
        {
            query = query.Where(s => s.StoreId == storeId);
        }

        var data = query
            .OrderBy(s => s.DateCreated)
            .Select(s => new
            {
                date = s.DateCreated.ToString("dd-MM-yyyy"),
                sold = s.Sold,
                quantity = s.Quantity,
                revenue = s.Revenue,
                profit = s.Profit
            })
            .ToList();

        return Json(data);
    }
}
