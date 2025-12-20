using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UniformPro.Core.Entities;
using UniformPro.Infrastructure.Data;
using UniformPro.Web.Services;
using UniformPro.Web.Helpers;
using UniformPro.Web.ViewModels;

namespace UniformPro.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;

        public ProductsController(ApplicationDbContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        // عرض المنتجات
        public async Task<IActionResult> Index(string searchString, int? pageNumber)
        {
            ViewData["CurrentFilter"] = searchString;

            var products = _context.Products.Include(p => p.Category).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(p => p.NameAr.Contains(searchString)
                                            || p.NameEn.Contains(searchString)
                                            || p.Category.NameAr.Contains(searchString));
            }

            products = products.OrderByDescending(p => p.CreatedAt);

            int pageSize = 10;
            var paginatedList = await PaginatedList<Product>.CreateAsync(products.AsNoTracking(), pageNumber ?? 1, pageSize);

       
            // لو الطلب AJAX (جاي من الجافاسكريبت) رجع الجدول بس
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_ProductTable", paginatedList);
            }

            // لو طلب عادي رجع الصفحة كاملة
            return View(paginatedList);
        }

        

        // ================== DELETE GALLERY IMAGE ==================
        // دالة لحذف صورة معينة من داخل صفحة التعديل
        [HttpPost]
        public async Task<IActionResult> DeleteImage(int id)
        {
            var image = await _context.ProductImages.FindAsync(id);
            if (image != null)
            {
                // حذف الملف من السيرفر
                _fileService.DeleteFile(image.ImagePath, "products/gallery");

                // حذف من الداتابيز
                _context.ProductImages.Remove(image);
                await _context.SaveChangesAsync();

                return Ok(); // نجاح
            }
            return NotFound();
        }
        // ================== CREATE ==================
        public IActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "NameAr");
            return View(new ProductViewModel()); // نرسل ViewModel فارغ
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductViewModel model)
        {
            if (ModelState.IsValid)
            {
                // تحويل ViewModel -> Entity
                var product = new Product
                {
                    NameAr = model.NameAr,
                    NameEn = model.NameEn,
                    StartPrice = model.StartPrice,
                    DescriptionAr = model.DescriptionAr,
                    DescriptionEn = model.DescriptionEn,
                    CategoryId = model.CategoryId,
                    MaterialDetailsAr = model.MaterialDetailsAr,
                    MaterialDetailsEn = model.MaterialDetailsEn,
                    AvailableSizes = model.AvailableSizes,
                    MinQuantity = model.MinQuantity,
                    MetaDescription = model.MetaDescription,
                    MetaKeywords = model.MetaKeywords,
                    CreatedAt = DateTime.Now

                };

                // حفظ الصورة الرئيسية
                if (model.MainImageFile != null)
                {
                    product.MainImagePath = await _fileService.SaveFileAsync(model.MainImageFile, "products");
                }

                _context.Add(product);
                await _context.SaveChangesAsync(); // للحصول على ID

                // حفظ صور المعرض
                if (model.GalleryFiles != null && model.GalleryFiles.Count > 0)
                {
                    foreach (var file in model.GalleryFiles)
                    {
                        var path = await _fileService.SaveFileAsync(file, "products/gallery");
                        _context.ProductImages.Add(new ProductImage
                        {
                            ProductId = product.Id,
                            ImagePath = path
                        });
                    }
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = "تم إضافة المنتج بنجاح";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "NameAr", model.CategoryId);
            return View(model);
        }

        // ================== EDIT ==================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products.Include(p => p.ProductImages).FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();

            // تحويل Entity -> ViewModel للعرض
            var model = new ProductViewModel
            {
                Id = product.Id,
                NameAr = product.NameAr,
                NameEn = product.NameEn,
                StartPrice = product.StartPrice,
                DescriptionAr = product.DescriptionAr,
                DescriptionEn = product.DescriptionEn,
                CategoryId = product.CategoryId,
                MaterialDetailsAr = product.MaterialDetailsAr,
                MaterialDetailsEn = product.MaterialDetailsEn,
                AvailableSizes = product.AvailableSizes,
                MinQuantity = product.MinQuantity,
                CurrentMainImagePath = product.MainImagePath,
                CurrentGalleryImages = product.ProductImages.ToList(),
                MetaDescription = product.MetaDescription,
                MetaKeywords = product.MetaKeywords
            };

            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "NameAr", model.CategoryId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
                if (product == null) return NotFound();

                // تحديث البيانات الأساسية
                product.NameAr = model.NameAr;
                product.NameEn = model.NameEn;
                product.StartPrice = model.StartPrice;
                product.DescriptionAr = model.DescriptionAr;
                product.DescriptionEn = model.DescriptionEn;
                product.CategoryId = model.CategoryId;
                product.MaterialDetailsAr = model.MaterialDetailsAr;
                product.MaterialDetailsEn = model.MaterialDetailsEn;
                product.AvailableSizes = model.AvailableSizes;
                product.MinQuantity = model.MinQuantity;
                product.MetaDescription = model.MetaDescription;
                product.MetaKeywords = model.MetaKeywords;


                // تحديث الصورة الرئيسية (إذا تم رفع جديد)
                if (model.MainImageFile != null)
                {
                    if (!string.IsNullOrEmpty(product.MainImagePath))
                        _fileService.DeleteFile(product.MainImagePath, "products");

                    product.MainImagePath = await _fileService.SaveFileAsync(model.MainImageFile, "products");
                }

                // إضافة صور معرض جديدة
                if (model.GalleryFiles != null && model.GalleryFiles.Count > 0)
                {
                    foreach (var file in model.GalleryFiles)
                    {
                        var path = await _fileService.SaveFileAsync(file, "products/gallery");
                        _context.ProductImages.Add(new ProductImage { ProductId = product.Id, ImagePath = path });
                    }
                }

                _context.Update(product);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "تم تحديث المنتج بنجاح";
                return RedirectToAction(nameof(Index));
            }

            // في حالة الخطأ، أعد تحميل الصور القديمة للعرض
            var existingProduct = await _context.Products.Include(p => p.ProductImages).AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
            if (existingProduct != null)
            {
                model.CurrentMainImagePath = existingProduct.MainImagePath;
                model.CurrentGalleryImages = existingProduct.ProductImages.ToList();
            }

            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "NameAr", model.CategoryId);
            return View(model);
        }
        // ================== DELETE PRODUCT ==================
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product != null)
            {
                // حذف الصورة الرئيسية
                if (!string.IsNullOrEmpty(product.MainImagePath))
                    _fileService.DeleteFile(product.MainImagePath, "products");

                // حذف صور المعرض
                foreach (var img in product.ProductImages)
                {
                    _fileService.DeleteFile(img.ImagePath, "products/gallery");
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}