using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Repository;

namespace Shopping_Tutorial.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class OrderController : Controller
{
    private readonly DataContext _dataContext;
    public OrderController(DataContext context)
    {
        _dataContext = context;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _dataContext.Orders.OrderByDescending(p => p.Id).ToListAsync());
    }

    public async Task<IActionResult> ViewOrder(string ordercode)
    {
        var detailsOrder = await _dataContext.OrderDetails.Include(od => od.Product).Where(od => od.OrderCode == ordercode).ToListAsync();

        return View(detailsOrder);
    }

    public async Task<IActionResult> Delete(int Id)
    {
        OrderModel order = await _dataContext.Orders.FindAsync(Id);
        _dataContext.Orders.Remove(order);
        await _dataContext.SaveChangesAsync();
        TempData["success"] = "Thương hiệu đã xóa thành công";
        return RedirectToAction("Index");
    }
}
