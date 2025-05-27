using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Models.ViewModels;
using Shopping_Tutorial.Repository;

namespace Shopping_Tutorial.Controllers;

public class CartController : Controller
{
    private readonly DataContext _dataContext;

    public CartController(DataContext dataContext)
    {
        _dataContext = dataContext;
    }

    public IActionResult Index()
    {
        var cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();

        var shippingPrice = GetShippingPriceFromCookie();
        var couponCode = GetCouponCodeFromCookie();

        decimal grandTotal = cartItems.Sum(x => x.Quantity * x.Price);
        ApplyCouponDiscount(ref grandTotal, couponCode, ref shippingPrice);

        var cartVM = new CartItemViewModel
        {
            CartItems = cartItems,
            GrandTotal = grandTotal,
            ShippingCost = shippingPrice,
            CouponCode = couponCode
        };

        return View(cartVM);
    }

    public IActionResult Checkout()
    {
        return View("~/Views/Checkout/Index.cshtml");
    }

    public async Task<IActionResult> Add(int id)
    {
        var product = await _dataContext.Products.FindAsync(id);
        if (product == null) return NotFound();

        var cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
        var existingItem = cart.FirstOrDefault(c => c.ProductId == id);

        if (existingItem == null)
            cart.Add(new CartItemModel(product));
        else
            existingItem.Quantity++;

        HttpContext.Session.SetJson("Cart", cart);
        TempData["success"] = "Thêm sản phẩm thành công";
        return Redirect(Request.Headers["Referer"].ToString());
    }

    [HttpPost]
    public async Task<IActionResult> Increase(int id)
    {
        var product = await _dataContext.Products.FindAsync(id);
        var cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");
        var item = cart?.FirstOrDefault(c => c.ProductId == id);

        if (item == null || product == null) return Json(new { success = false });

        if (product.Quantity > item.Quantity)
        {
            item.Quantity++;
        }

        HttpContext.Session.SetJson("Cart", cart);
        return Json(new
        {
            success = true,
            quantity = item.Quantity,
            total = item.Quantity * item.Price,
            grandTotal = cart.Sum(x => x.Quantity * x.Price)
        });
    }

    [HttpPost]
    public IActionResult Decrease(int id)
    {
        var cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");
        var item = cart?.FirstOrDefault(c => c.ProductId == id);

        if (item != null && item.Quantity > 1)
        {
            item.Quantity--;
            HttpContext.Session.SetJson("Cart", cart);
        }

        return Json(new
        {
            success = true,
            quantity = item?.Quantity ?? 0,
            total = item?.Quantity * item?.Price ?? 0,
            grandTotal = cart?.Sum(x => x.Price * x.Quantity) ?? 0
        });
    }

    public IActionResult Remove(int id)
    {
        var cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new();
        cart.RemoveAll(c => c.ProductId == id);

        if (cart.Count == 0)
            HttpContext.Session.Remove("Cart");
        else
            HttpContext.Session.SetJson("Cart", cart);

        TempData["success"] = "Xóa sản phẩm thành công";
        return RedirectToAction("Index");
    }

    public IActionResult Clear()
    {
        HttpContext.Session.Remove("Cart");
        TempData["success"] = "Xóa tất cả sản phẩm thành công";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> AddAjax(int id)
    {
        var product = await _dataContext.Products.FindAsync(id);
        if (product == null) return Json(new { success = false, message = "Không tìm thấy sản phẩm." });

        var cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new();
        var existingItem = cart.FirstOrDefault(c => c.ProductId == id);

        if (existingItem == null)
            cart.Add(new CartItemModel(product));
        else
            existingItem.Quantity++;

        HttpContext.Session.SetJson("Cart", cart);
        return Json(new { success = true, message = "Đã thêm sản phẩm vào giỏ hàng!" });
    }

    [HttpPost]
    public async Task<IActionResult> GetShipping(string quan, string tinh, string phuong)
    {
        var shipping = await _dataContext.Shippings
            .FirstOrDefaultAsync(x => x.City == tinh && x.District == quan && x.Ward == phuong);

        decimal price = shipping?.Price ?? 50000;

        try
        {
            var options = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTimeOffset.Now.AddMinutes(30),
                Secure = Request.IsHttps
            };
            Response.Cookies.Append("ShippingPrice", JsonConvert.SerializeObject(price), options);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Shipping cookie error: " + ex.Message);
        }

        return Json(new { shippingPrice = price });
    }

    [HttpPost]
    public IActionResult RemoveShippingCookie()
    {
        Response.Cookies.Delete("ShippingPrice");
        return Json(new { success = true });
    }

    [HttpPost]
    public async Task<IActionResult> GetCoupon([FromForm] string coupon_value)
    {
        var coupon = await _dataContext.Coupons.FirstOrDefaultAsync(x => x.Name == coupon_value);

        if (coupon == null)
            return Ok(new { success = false, message = "Mã giảm giá không tồn tại" });

        if (coupon.DateEnd < DateTime.Now)
            return Ok(new { success = false, message = "Mã giảm giá đã hết hạn" });

        string title = coupon.Name + " | " + coupon.Description;

        var options = new CookieOptions
        {
            HttpOnly = true,
            Expires = DateTimeOffset.UtcNow.AddMinutes(30),
            Secure = false,
            SameSite = SameSiteMode.Strict
        };

        try
        {
            Response.Cookies.Append("CouponTitle", title, options);
            return Ok(new { success = true, message = "Mã giảm giá áp dụng thành công" });
        }
        catch (Exception ex)
        {
            Console.WriteLine("Coupon cookie error: " + ex.Message);
            return Ok(new { success = false, message = "Mã giảm giá áp dụng thất bại" });
        }
    }

    private decimal GetShippingPriceFromCookie()
    {
        var cookie = Request.Cookies["ShippingPrice"];
        return !string.IsNullOrEmpty(cookie) ? JsonConvert.DeserializeObject<decimal>(cookie) : 0;
    }

    private string GetCouponCodeFromCookie()
    {
        return Request.Cookies["CouponTitle"];
    }

    private void ApplyCouponDiscount(ref decimal total, string couponCode, ref decimal shipping)
    {
        if (string.IsNullOrEmpty(couponCode)) return;

        if (couponCode.StartsWith("FREESHIP", StringComparison.OrdinalIgnoreCase))
        {
            shipping = 0;
        }
        else if (couponCode.StartsWith("DISCOUNT500K", StringComparison.OrdinalIgnoreCase))
        {
            total -= 500000;
        }
        else if (couponCode.StartsWith("DISCOUNT200K", StringComparison.OrdinalIgnoreCase))
        {
            total -= 200000;
        }

        if (total < 0)
            total = 0;
    }
}
