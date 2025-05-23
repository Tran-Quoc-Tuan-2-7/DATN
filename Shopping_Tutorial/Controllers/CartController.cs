using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
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
        List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();

        // Nhận shipping giá từ cookie
        var shippingPriceCookie = Request.Cookies["ShippingPrice"];
        decimal shippingPrice = 0;
        if (shippingPriceCookie != null)
        {
            var shippingPriceJson = shippingPriceCookie;
            shippingPrice = JsonConvert.DeserializeObject<decimal>(shippingPriceJson);
        }

        // Nhận coupon code
        var coupon_code = Request.Cookies["CouponTitle"];

        // Nếu áp dụng mã FREESHIP, phí vận chuyển = 0
        if (!string.IsNullOrEmpty(coupon_code) && coupon_code.StartsWith("FREESHIP", StringComparison.OrdinalIgnoreCase))
        {
            shippingPrice = 0;
        }

        decimal grandTotal = cartItems.Sum(x => x.Quantity * x.Price);

        // Nếu áp dụng mã DISCOUNT500K, giảm 500.000đ
        if (!string.IsNullOrEmpty(coupon_code) && coupon_code.StartsWith("DISCOUNT500K", StringComparison.OrdinalIgnoreCase))
        {
            grandTotal -= 500000;

            // Đảm bảo không âm
            if (grandTotal < 0)
                grandTotal = 0;
        }

        if (!string.IsNullOrEmpty(coupon_code) && coupon_code.StartsWith("DISCOUNT200K", StringComparison.OrdinalIgnoreCase))
        {
            grandTotal -= 200000;

            // Đảm bảo không âm
            if (grandTotal < 0)
                grandTotal = 0;
        }

        CartItemViewModel cartVM = new()
        {
            CartItems = cartItems,
            GrandTotal = grandTotal,
            ShippingCost = shippingPrice,
            CouponCode = coupon_code
        };

        return View(cartVM);
    }



    public IActionResult Checkout()
    {
        return View("~/Views/Checkout/Index.cshtml");
    }

    public async Task<IActionResult> Add(int Id)
    {
        ProductModel product = await _dataContext.Products.FindAsync(Id);

        List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();

        CartItemModel cartItems = cart.Where(c => c.ProductId == Id).FirstOrDefault();

        if (cartItems == null)
        {
            cart.Add(new CartItemModel(product));
        }

        else
        {
            cartItems.Quantity += 1;
        }

        HttpContext.Session.SetJson("Cart", cart);

        TempData["success"] = "Thêm sản phẩm thành công";

        return Redirect(Request.Headers["Referer"].ToString());
    }

    //Ham giam so luong
    [HttpPost]
    public async Task<IActionResult> Increase(int id)
    {
        var product = await _dataContext.Products.FirstOrDefaultAsync(p => p.Id == id);
        var cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");
        var cartItem = cart.FirstOrDefault(c => c.ProductId == id);

        if (cartItem.Quantity > 0 && product.Quantity > cartItem.Quantity)
        {
            cartItem.Quantity++;
        }

        HttpContext.Session.SetJson("Cart", cart);

        return Json(new
        {
            success = true,
            quantity = cartItem.Quantity,
            total = cartItem.Quantity * cartItem.Price,
            grandTotal = cart.Sum(x => x.Price * x.Quantity)
        });
    }

    [HttpPost]
    public async Task<IActionResult> Decrease(int id)
    {
        var cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");
        var cartItem = cart.FirstOrDefault(c => c.ProductId == id);

        if (cartItem.Quantity > 1)
        {
            cartItem.Quantity--;
        }

        HttpContext.Session.SetJson("Cart", cart);

        return Json(new
        {
            success = true,
            quantity = cartItem.Quantity,
            total = cartItem.Quantity * cartItem.Price,
            grandTotal = cart.Sum(x => x.Price * x.Quantity)
        });
    }


    //Ham Remove
    public async Task<IActionResult> Remove(int Id)
    {
        List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");
        cart.RemoveAll(p => p.ProductId == Id);

        if (cart.Count == 0)
        {
            HttpContext.Session.Remove("Cart");
        }

        else
        {
            HttpContext.Session.SetJson("Cart", cart);

        }

        TempData["success"] = "Xóa sản phẩm thành công";

        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Clear(int Id)
    {
        HttpContext.Session.Remove("Cart");

        TempData["success"] = "Xóa tất cả sản phẩm thành công";

        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> AddAjax(int Id)
    {
        ProductModel product = await _dataContext.Products.FindAsync(Id);

        if (product == null)
            return Json(new { success = false, message = "Không tìm thấy sản phẩm." });

        List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();

        CartItemModel cartItem = cart.FirstOrDefault(c => c.ProductId == Id);

        if (cartItem == null)
            cart.Add(new CartItemModel(product));
        else
            cartItem.Quantity += 1;

        HttpContext.Session.SetJson("Cart", cart);

        return Json(new { success = true, message = "Đã thêm sản phẩm vào giỏ hàng!" });
    }

    //tinh phi ship
    [HttpPost]
    public async Task<IActionResult> GetShipping(ShippingModel shippingModel, string quan, string tinh, string phuong)
    {
        var existingShipping = await _dataContext.Shippings
            .FirstOrDefaultAsync(x => x.City == tinh && x.District == quan && x.Ward == phuong);
        decimal shippingPrice = 0;
        if (existingShipping != null)
        {
            shippingPrice = existingShipping.Price;
        }
        else
        {
            shippingPrice = 50000;
        }
        var shippingPriceJson = JsonConvert.SerializeObject(shippingPrice);
        try
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTimeOffset.Now.AddMinutes(30),
                Secure = Request.IsHttps
            };
            Response.Cookies.Append("ShippingPrice", shippingPriceJson, cookieOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding shipping price cookie: {ex.Message}");
        }

        return Json(new { shippingPrice });
    }

    [HttpPost]
    public IActionResult RemoveShippingCookie()
    {
        Response.Cookies.Delete("ShippingPrice");
        return Json(new { success = true });
    }

    //Ham GetCoupon Code
    [HttpPost]
    public async Task<IActionResult> GetCoupon([FromForm] string coupon_value)
    {
        var validCoupon = await _dataContext.Coupons
            .FirstOrDefaultAsync(x => x.Name == coupon_value);

        if (validCoupon == null)
        {
            return Ok(new { success = false, message = "Mã giảm giá không tồn tại" });
        }

        string couponTitle = validCoupon.Name + " | " + validCoupon.Description;

        TimeSpan remainingTime = validCoupon.DateEnd - DateTime.Now;
        int daysRemaining = remainingTime.Days;

        if (daysRemaining >= 0)
        {
            try
            {
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(30),
                    Secure = false,
                    SameSite = SameSiteMode.Strict
                };
                Response.Cookies.Append("CouponTitle", couponTitle, cookieOptions);
                return Ok(new { success = true, message = "Mã giảm giá áp dụng thành công" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding coupon cookie: {ex.Message}");
                return Ok(new { success = false, message = "Mã giảm giá áp dụng thất bại" });
            }
        }
        else
        {
            return Ok(new { success = false, message = "Mã giảm giá đã hết hạn" });
        }
    }
}