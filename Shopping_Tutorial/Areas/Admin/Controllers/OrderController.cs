using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Repository;

namespace Shopping_Tutorial.Areas.Admin.Controllers
{
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
            var orders = await _dataContext.Orders
                .Include(o => o.Store)
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            var stores = await _dataContext.Stores.ToListAsync();
            ViewBag.Stores = stores;

            return View(orders);
        }


        public async Task<IActionResult> ViewOrder(string ordercode)
        {
            var detailsOrder = await _dataContext.OrderDetails
                .Include(od => od.Product)
                .Where(od => od.OrderCode == ordercode)
                .ToListAsync();

            var order = await _dataContext.Orders
                .Include(o => o.Store)
                .FirstOrDefaultAsync(o => o.OrderCode == ordercode);

            if (order == null)
                return NotFound();

            ViewBag.ShippingCost = order.ShippingCost;
            ViewBag.Status = order.Status;
            ViewBag.StoreName = order.Store?.Name ?? "Không xác định";
            ViewBag.Stores = await _dataContext.Stores.ToListAsync(); // Nếu muốn hiển thị dropdown chọn cơ sở

            return View(detailsOrder);
        }

        public async Task<IActionResult> Delete(int Id)
        {
            var order = await _dataContext.Orders.FindAsync(Id);
            if (order == null)
                return NotFound();

            _dataContext.Orders.Remove(order);
            await _dataContext.SaveChangesAsync();

            TempData["success"] = "Đơn hàng đã xóa thành công";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrder(string ordercode, int status, int? storeId = null)
        {
            var order = await _dataContext.Orders.FirstOrDefaultAsync(o => o.OrderCode == ordercode);
            if (order == null)
                return NotFound();

            if (storeId.HasValue && storeId.Value != order.StoreId)
            {
                var storeExists = await _dataContext.Stores.AnyAsync(s => s.Id == storeId.Value);
                if (!storeExists)
                    return BadRequest(new { success = false, message = "Cơ sở không hợp lệ" });

                order.StoreId = storeId.Value;
            }

            if (status == 2 && order.Status != 2)
            {
                var orderDetails = await _dataContext.OrderDetails
                    .Where(od => od.OrderCode == ordercode)
                    .Include(od => od.Product)
                    .ToListAsync();

                foreach (var item in orderDetails)
                {
                    var product = item.Product;
                    if (product.Quantity < item.Quantity)
                        return BadRequest(new { success = false, message = $"Không đủ số lượng sản phẩm {product.Name}" });

                    product.Quantity -= item.Quantity;
                    product.Sold += item.Quantity;
                    _dataContext.Update(product);
                }

                // Tính toán doanh thu thực tế
                decimal baseRevenue = orderDetails.Sum(od => od.Quantity * od.Product.Price);
                decimal shippingCost = order.ShippingCost;
                decimal discountAmount = 0;

                if (!string.IsNullOrEmpty(order.CouponCode))
                {
                    switch (order.CouponCode.ToLower())
                    {
                        case "discount500k":
                            discountAmount = 500000;
                            break;
                        case "discount200k":
                            discountAmount = 200000;
                            break;
                        case "freeship":
                            shippingCost = 0;
                            break;
                    }
                }

                decimal totalRevenue = baseRevenue + shippingCost - discountAmount;
                decimal totalProfit = orderDetails.Sum(od => (od.Product.Price / 5) * od.Quantity); // giữ nguyên

                // Cập nhật thống kê
                var statistical = await _dataContext.Statisticals
                    .FirstOrDefaultAsync(s => s.DateCreated.Date == order.CreatedDate.Date && s.StoreId == order.StoreId);

                int totalQuantity = orderDetails.Count;
                int totalSold = orderDetails.Sum(od => od.Quantity);

                if (statistical != null)
                {
                    statistical.Quantity += totalQuantity;
                    statistical.Sold += totalSold;
                    statistical.Revenue += totalRevenue;
                    statistical.Profit += totalProfit;
                    _dataContext.Update(statistical);
                }
                else
                {
                    statistical = new StatisticalModel()
                    {
                        DateCreated = order.CreatedDate,
                        StoreId = order.StoreId,
                        Quantity = totalQuantity,
                        Sold = totalSold,
                        Revenue = totalRevenue,
                        Profit = totalProfit
                    };
                    _dataContext.Add(statistical);
                }

                order.Status = status;
            }

            try
            {
                await _dataContext.SaveChangesAsync();
                return Ok(new { success = true, message = "Cập nhật đơn hàng thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi: {ex.Message}");
            }
        }
    }
}
