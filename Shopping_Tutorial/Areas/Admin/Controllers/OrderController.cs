using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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

    [HttpPost]
    public async Task<IActionResult> UpdateOrder(string ordercode, int status)
    {
        var order = await _dataContext.Orders.FirstOrDefaultAsync(o => o.OrderCode == ordercode);
        if (order == null)
        {
            return NotFound();
        }

        order.Status = status;
        try
        {
            await _dataContext.SaveChangesAsync();
            return Ok(new { success = true, message = "Cập nhật trạng thái đơn hàng thành công" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Xảy ra lỗi khi cập nhật trạng thái đơn hàng");
        }
    }
}
