using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Repository;

namespace Shopping_Tutorial.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ProductController : Controller
{
    private readonly DataContext _dataContext;
    private readonly IWebHostEnvironment _webHostEnvironment;
    public ProductController(DataContext context, IWebHostEnvironment webHostEnvironment)
    {
        _dataContext = context;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _dataContext.Products.OrderByDescending(p => p.Id).Include(p => p.Category).Include(p => p.Brand).ToArrayAsync());
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Categories = new SelectList(_dataContext.Categories, "Id","Name");
        ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductModel product)
    {
        ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryId);
        ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandId);

        if (ModelState.IsValid)
        {
            product.Slug = product.Name.Replace(" ", "-");
            var slug = await _dataContext.Products.FirstOrDefaultAsync(p => p.Slug == product.Slug);
            if (slug != null)
            {
                ModelState.AddModelError("", "Sản phẩm đã có trong database");
                return View(product);
            }
            if (product.ImageUpload != null)
            {
                string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");
                string imageName = Guid.NewGuid().ToString() + "_" + product.ImageUpload.FileName;
                string filePath = Path.Combine(uploadDir, imageName);

                FileStream fs = new FileStream(filePath, FileMode.Create);
                await product.ImageUpload.CopyToAsync(fs);
                fs.Close();
                product.Image = imageName;
            }
            _dataContext.Add(product);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Thêm sản phẩm thành công";
            return RedirectToAction("Index");
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
            TempData["error"] = "Có một vài lỗi trong form, vui lòng kiểm tra lại.";
            return View(product);
        }
        return View(product);
    }

    public async Task<IActionResult> Edit(int Id)
    {
        ProductModel product = await _dataContext.Products.FindAsync(Id);
        ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryId);
        ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandId);

        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int Id, ProductModel product)
    {
        ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryId);
        ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandId);

        var existed_product = _dataContext.Products.Find(product.Id); // tim san pham theo id

        if (ModelState.IsValid)
        {
            product.Slug = product.Name.Replace(" ", "-");
            var slug = await _dataContext.Products.FirstOrDefaultAsync(p => p.Slug == product.Slug);
            if (slug != null)
            {
                ModelState.AddModelError("", "Sản phẩm đã có trong database");
                return View(product);
            }
            if (product.ImageUpload != null)
            {
                //hinh anh moi
                string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");
                string imageName = Guid.NewGuid().ToString() + "_" + product.ImageUpload.FileName;
                string filePath = Path.Combine(uploadDir, imageName);

                //xóa hinh anh cu
                string oldFilePath = Path.Combine(uploadDir, existed_product.Image);
                try
                {
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Đã xảy ra lỗi khi xóa hình ảnh sản phẩm");
                }

                FileStream fs = new FileStream(filePath, FileMode.Create);
                await product.ImageUpload.CopyToAsync(fs);
                fs.Close();
                existed_product.Image = imageName;

                
            }
            //cap nhat thuoc tinh cua san pham
            existed_product.Name = product.Name;
            existed_product.Description = product.Description;
            existed_product.Price = product.Price;
            existed_product.BrandId = product.BrandId;
            existed_product.CategoryId = product.CategoryId;

            _dataContext.Update(existed_product);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Cập nhật sản phẩm thành công";
            return RedirectToAction("Index");
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
            TempData["error"] = "Có một vài lỗi trong form, vui lòng kiểm tra lại.";
            return View(product);
        }
        return View(product);
    }

    public async Task<IActionResult> Delete(int Id)
    {
        ProductModel product = await _dataContext.Products.FindAsync(Id);
        if(product == null)
        {
            return NotFound();
        }

        string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");
        string oldFilePath = Path.Combine(uploadDir, product.Image);
        try
        {
            if (System.IO.File.Exists(oldFilePath))
            {
                System.IO.File.Delete(oldFilePath);
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Đã xảy ra lỗi khi xóa hình ảnh sản phẩm");
        }
        _dataContext.Products.Remove(product);
        await _dataContext.SaveChangesAsync();
        TempData["success"] = "Sản phẩm đã xóa thành công";
        return RedirectToAction("Index");
    }

}



