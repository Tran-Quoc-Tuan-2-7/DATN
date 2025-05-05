using System.Diagnostics.Eventing.Reader;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Repository;

namespace Shopping_Tutorial.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class UserController : Controller
{
    private readonly UserManager<AppUserModel> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly DataContext _dataContext;

    public UserController(UserManager<AppUserModel> userManager, RoleManager<IdentityRole> roleManager, DataContext dataContext )
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _dataContext = dataContext;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userWithRoles = await(from u in  _dataContext.Users
                                  join ur in _dataContext.UserRoles on u.Id equals ur.UserId
                                  join r in  _dataContext.Roles on ur.RoleId equals r.Id
                                  select new { User = u, RoleName = r.Name })
                                  .ToListAsync();

        var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        ViewBag.LoggedInUserId = loggedInUserId;
        return View(userWithRoles);
    }


    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var roles = await _roleManager.Roles.ToListAsync();
        ViewBag.Roles = new SelectList(roles, "Id", "Name");

        return View(new AppUserModel());
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return NotFound();
        }
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        var roles = await _roleManager.Roles.ToListAsync();
        ViewBag.Roles = new SelectList(roles, "Id", "Name");

        return View(user);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(string id, AppUserModel user)
    {
        var existingUser = await _userManager.FindByIdAsync(id);
        if (existingUser == null)
        {
            return NotFound();
        }
        if (ModelState.IsValid)
        {
            existingUser.UserName = user.UserName;
            existingUser.Email = user.Email;
            existingUser.PhoneNumber = user.PhoneNumber;
            existingUser.RoleId = user.RoleId;
            var updateUserResult = await _userManager.UpdateAsync(existingUser);
            if (updateUserResult.Succeeded)
            {
                return RedirectToAction("Index", "User");
            }
            else
            {
                AddIdentityErrors(updateUserResult);
                return View(existingUser);
            }
        }

        var roles = await _roleManager.Roles.ToListAsync();
        ViewBag.Roles = new SelectList(roles, "Id", "Name");
        TempData["Title"] = "Model validation failed";
        var errors = ModelState.Values.Select(v => v.Errors.Select(e => e.ErrorMessage).ToList());
        string errorMessage = string.Join("\n", errors);
        return View(existingUser);

    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AppUserModel user)
    {
        if (ModelState.IsValid)
        {
            var createUserResult = await _userManager.CreateAsync(user, user.PasswordHash); //tao user
            if (createUserResult.Succeeded)
            {
                var createUser = await _userManager.FindByEmailAsync(user.Email); // tim user dua vao email
                var userId = createUser.Id; // lay user id
                var role = _roleManager.FindByIdAsync(user.RoleId); //lay roleid
                //gan quyen
                var addToRoleResult = await _userManager.AddToRoleAsync(createUser, role.Result.Name);
                if (!addToRoleResult.Succeeded)
                {
                    foreach(var error in createUserResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
                return RedirectToAction("Index", "User", new { area = "Admin" });
            }
            else
            {
                AddIdentityErrors(createUserResult);
                return View(user);
            }

        }
        else
        {
            TempData["error"] = "Có một vài thứ đang bị lỗi";
            List<string> errors = new List<string>();
            foreach (var value in ModelState.Values)
            {
                foreach (var error in value.Errors)
                {
                    errors.Add(error.ErrorMessage);
                }
            }
            string errorMessage = string.Join("\n", errors);
            return BadRequest(errorMessage);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Delete(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return NotFound();
        }
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        var deleteResult = await _userManager.DeleteAsync(user);
        if (!deleteResult.Succeeded)
        {
            return View("Error");
        }
        TempData["success"] = "Xóa người dùng thành công";
        return RedirectToAction("Index");
    }

    private void AddIdentityErrors(IdentityResult identityResult)
    {
        foreach (var error in identityResult.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }
}
