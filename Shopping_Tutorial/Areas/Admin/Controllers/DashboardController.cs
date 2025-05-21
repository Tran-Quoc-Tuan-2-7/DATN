using Microsoft.AspNetCore.Mvc;
using Shopping_Tutorial.Repository;
using Shopping_Tutorial.Models;

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

        return View();
    }

    [HttpPost]
    public IActionResult GetChartData()
    {
        var data = _dataContext.Statisticals
            .OrderBy(s => s.DateCreated) // 🟢 Sắp xếp theo ngày tăng dần
            .Select(s => new
            {
                date = s.DateCreated.ToString("dd-MM-yyyy"), // Trả về DateTime gốc
                sold = s.Sold,
                quantity = s.Quantity,
                revenue = s.Revenue,
                profit = s.Profit
            })
            .ToList();

        return Json(data);
    }


    [HttpPost]
    public async Task<IActionResult> GetChartDataBySelect(DateTime startDate, DateTime endDate)
    {
        var data = _dataContext.Statisticals
            .Where(s => s.DateCreated.Date >= startDate.Date && s.DateCreated.Date <= endDate.Date)
            .OrderBy(s => s.DateCreated) // Sắp xếp để trục X đúng thứ tự
            .Select(s => new
            {
                date = s.DateCreated.ToString("dd-MM-yyyy"), // Trả về chuỗi ngày
                sold = s.Sold,
                quantity = s.Quantity,
                revenue = s.Revenue,
                profit = s.Profit
            })
            .ToList();

        return Json(data);
    }

}
