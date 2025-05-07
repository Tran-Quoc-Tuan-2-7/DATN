using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Repository;

namespace Shopping_Tutorial.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ContactController : Controller
{
    private readonly DataContext _dataContext;
    private readonly IWebHostEnvironment _webHostEnvironment;
    public ContactController(DataContext context, IWebHostEnvironment webHostEnvironment)
    {
        _dataContext = context;
        _webHostEnvironment = webHostEnvironment;
    }
    public IActionResult Index()
    {
        var contact = _dataContext.Contacts.ToList();
        return View(contact);
    }

    public async Task<IActionResult> Edit()
    {
        ContactModel contact = await _dataContext.Contacts.FirstOrDefaultAsync();
        return View(contact);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int Id, ContactModel contact)
    {
        var existed_contact = await _dataContext.Contacts.FirstOrDefaultAsync();

        if (existed_contact == null)
            return NotFound();

        if (ModelState.IsValid)
        {
            if (contact.ImageUpload != null)
            {
                string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/logo");
                string imageName = Guid.NewGuid().ToString() + "_" + contact.ImageUpload.FileName;
                string filePath = Path.Combine(uploadDir, imageName);

                using (var fs = new FileStream(filePath, FileMode.Create))
                {
                    await contact.ImageUpload.CopyToAsync(fs);
                }

                existed_contact.LogoImg = imageName;
            }

            // Cập nhật các thuộc tính
            existed_contact.Name = contact.Name;
            existed_contact.Description = contact.Description;
            existed_contact.Phone = contact.Phone;
            existed_contact.Email = contact.Email;
            existed_contact.Map = contact.Map;

            _dataContext.Update(existed_contact);
            await _dataContext.SaveChangesAsync();

            TempData["success"] = "Cập nhật thông tin liên hệ thành công";
            return RedirectToAction("Index");
        }
        else
        {
            TempData["error"] = "Có một vài lỗi trong form, vui lòng kiểm tra lại.";
            return View(existed_contact); // Trả lại object từ DB, không phải object bind từ form
        }
    }
}
