using System.Net.Http.Headers;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Models.ViewModels;
using Shopping_Tutorial.Repository;

namespace Shopping_Tutorial.Controllers;

public class AccountController : Controller
{
    private UserManager<AppUserModel> _userManager;
    private SignInManager<AppUserModel> _signInManager;
    private readonly DataContext _dataContext;

    public AccountController(SignInManager<AppUserModel> signInManager, UserManager<AppUserModel> userManager, DataContext context)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _dataContext = context;
    }

    public IActionResult Login(string returnUrl)
    {
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel loginVM)
    {
        if (ModelState.IsValid)
        {
            var result = await _signInManager.PasswordSignInAsync(loginVM.Username, loginVM.Password, false, false);

            if (result.Succeeded)
            {
                return Redirect(loginVM.ReturnUrl ?? "/");
            }

            ModelState.AddModelError("", "Username hoặc mật khẩu không hợp lệ!");
        }

        return View(loginVM);
    }



    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(UserModel user)
    {
        if (ModelState.IsValid)
        {
            AppUserModel newUser = new AppUserModel
            {
                UserName = user.Username,
                Email = user.Email,
            };
            IdentityResult result = await _userManager.CreateAsync(newUser, user.Password);
            if (result.Succeeded)
            {
                TempData["success"] = "Tạo user thành công!";
                return Redirect("/account/login");
            }
            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }
        return View(user);
    }

    public async Task<IActionResult> Logout(string returnUrl = "/")
    {
        await _signInManager.SignOutAsync();

        return Redirect(returnUrl);
    }

    public async Task<IActionResult> History()
    {
        if ((bool)!User.Identity?.IsAuthenticated)
        {
            return RedirectToAction("Login", "Account");
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userEmail = User.FindFirstValue(ClaimTypes.Email);

        var orders = await _dataContext.Orders
            .Where(od => od.UserName == userEmail).OrderByDescending(od => od.Id).ToListAsync();
        ViewBag.Orders = orders;

        return View(orders);
    }

    public async Task<IActionResult> CancelOrder(string ordercode)
    {
        if ((bool)!User.Identity?.IsAuthenticated)
        {
            return RedirectToAction("Login", "Account");
        }

        try
        {
            var order = await _dataContext.Orders
                .FirstOrDefaultAsync(o => o.OrderCode == ordercode);

            if (order == null)
            {
                return NotFound("Không tìm thấy đơn hàng.");
            }

            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            // Chỉ hủy đơn khi đang ở trạng thái "Đơn hàng mới"
            if (order.Status != 1)
            {
                TempData["error"] = "Không thể hủy đơn hàng đã xử lý hoặc đã hủy!";
                return RedirectToAction("History");
            }

            order.Status = 3; // Giả sử 3 là trạng thái "Đã hủy"
            _dataContext.Orders.Update(order);
            await _dataContext.SaveChangesAsync();

            TempData["success"] = "Đã hủy đơn hàng thành công.";
        }
        catch (Exception ex)
        {
            TempData["error"] = "Lỗi khi hủy đơn hàng: " + ex.Message;
        }

        return RedirectToAction("History");
    }

}
