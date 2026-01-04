using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UniformPro.Core.Entities;
using UniformPro.Infrastructure.Data;
using UniformPro.Web.Services;
using Microsoft.AspNetCore.OutputCaching; // Added
using UniformPro.Web.Helpers;
using UniformPro.Web.ViewModels;

namespace UniformPro.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize]
    [AllowAnonymous]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;
        private readonly IHtmlSanitizerService _sanitizer;
        private readonly ILogger<ProductsController> _logger;
        private readonly IOutputCacheStore _cacheStore; // Added

        public ProductsController(ApplicationDbContext context, IFileService fileService, IHtmlSanitizerService sanitizer, ILogger<ProductsController> logger, IOutputCacheStore cacheStore)
        {
            _context = context;
            _fileService = fileService;
            _sanitizer = sanitizer;
            _logger = logger;
            _cacheStore = cacheStore; // Added
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

        

        // ================== CREATE ==================
        public IActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "NameAr");
            return View(new ProductViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var product = new Product
                    {
                        NameAr = model.NameAr,
                        NameEn = model.NameEn,
                        StartPrice = model.StartPrice,
                        DescriptionAr = _sanitizer.Sanitize(model.DescriptionAr),
                        DescriptionEn = _sanitizer.Sanitize(model.DescriptionEn),
                        CategoryId = model.CategoryId,
                        MaterialDetailsAr = _sanitizer.Sanitize(model.MaterialDetailsAr),
                        MaterialDetailsEn = _sanitizer.Sanitize(model.MaterialDetailsEn),
                        AvailableSizes = model.AvailableSizes,
                        MinQuantity = model.MinQuantity,
                        ShowOnHome = model.ShowOnHome,
                        MetaDescription = model.MetaDescription,
                        MetaKeywords = model.MetaKeywords,
                        CreatedAt = DateTime.Now
                    };

                    // رفع الصورة الرئيسية
                    if (model.MainImageFile != null)
                    {
                        try
                        {
                            product.MainImagePath = await _fileService.SaveFileAsync(model.MainImageFile, Constants.Folders.Products);
                        }
                        catch (ArgumentException ex)
                        {
                            _logger.LogError(ex, "Error uploading main image in Create Product");
                            ModelState.AddModelError("MainImageFile", ex.Message);
                            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "NameAr", model.CategoryId);
                            return View(model);
                        }
                    }

                    _context.Add(product);
                    await _context.SaveChangesAsync();

                    // Evict caches
                    await _cacheStore.EvictByTagAsync("products_data", CancellationToken.None);
                    await _cacheStore.EvictByTagAsync("home_data", CancellationToken.None);

                    // رفع صور المعرض
                    if (model.GalleryFiles != null && model.GalleryFiles.Count > 0)
                    {
                        try
                        {
                            foreach (var file in model.GalleryFiles)
                            {
                                var path = await _fileService.SaveFileAsync(file, Constants.Folders.ProductsGallery);
                                _context.ProductImages.Add(new ProductImage { ProductId = product.Id, ImagePath = path });
                            }
                            await _context.SaveChangesAsync();
                        }
                        catch (ArgumentException ex)
                        {
                             _logger.LogError(ex, "Error uploading gallery images in Create Product");
                             TempData["ErrorMessage"] = $"تم حفظ المنتج ولكن فشل رفع بعض صور المعرض: {ex.Message}";
                        }
                    }

                    TempData["SuccessMessage"] = "تم إضافة المنتج بنجاح";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in {ActionName}", nameof(Create));
                return Redirect("/Admin/Error/General");
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
                ShowOnHome = product.ShowOnHome,
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
        public async Task<IActionResult> Edit(int id, ProductViewModel model, string? deletedGalleryImageIds, bool deleteMainImage = false)
        {
            if (id != model.Id) return NotFound();

            try
            {
                if (ModelState.IsValid)
                {
                    var product = await _context.Products.Include(p => p.ProductImages).FirstOrDefaultAsync(p => p.Id == id);
                    if (product == null) return NotFound();

                    // 1. معالجة الصور المحذوفة (Deferred Deletion)
                    if (!string.IsNullOrEmpty(deletedGalleryImageIds))
                    {
                        var idsToDelete = deletedGalleryImageIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                                                .Select(i => int.Parse(i)).ToList();

                        var imagesToDelete = product.ProductImages.Where(img => idsToDelete.Contains(img.Id)).ToList();
                        
                        foreach (var img in imagesToDelete)
                        {
                            // حذف الملف الفعلي
                            _fileService.DeleteFile(img.ImagePath, Constants.Folders.ProductsGallery);
                            // حذف من الداتابيز
                            _context.ProductImages.Remove(img);
                        }
                    }

                    // تحديث البيانات الأساسية
                    product.NameAr = model.NameAr;
                    product.NameEn = model.NameEn;
                    product.StartPrice = model.StartPrice;
                    product.DescriptionAr = _sanitizer.Sanitize(model.DescriptionAr);
                    product.DescriptionEn = _sanitizer.Sanitize(model.DescriptionEn);
                    product.CategoryId = model.CategoryId;
                    product.MaterialDetailsAr = _sanitizer.Sanitize(model.MaterialDetailsAr);
                    product.MaterialDetailsEn = _sanitizer.Sanitize(model.MaterialDetailsEn);
                    product.AvailableSizes = model.AvailableSizes;
                    product.MinQuantity = model.MinQuantity;
                    product.ShowOnHome = model.ShowOnHome;
                    product.MetaDescription = model.MetaDescription;
                    product.MetaKeywords = model.MetaKeywords;


                    // 2. معالجة حذف الصورة الرئيسية (Deferred)
                    if (deleteMainImage && !string.IsNullOrEmpty(product.MainImagePath))
                    {
                         _fileService.DeleteFile(product.MainImagePath, Constants.Folders.Products);
                         product.MainImagePath = null;
                    }

                    // تحديث الصورة الرئيسية (إذا تم رفع جديد)
                    if (model.MainImageFile != null)
                    {
                        try
                        {
                            // الحذف يتم التعامل معه داخل السرفيس إذا مررنا الاسم
                            if (!string.IsNullOrEmpty(product.MainImagePath))
                                _fileService.DeleteFile(product.MainImagePath, Constants.Folders.Products);
                                
                            product.MainImagePath = await _fileService.SaveFileAsync(model.MainImageFile, Constants.Folders.Products);
                        }
                        catch (ArgumentException ex)
                        {
                            _logger.LogError(ex, "Error uploading main image in Edit Product");
                            ModelState.AddModelError("MainImageFile", ex.Message);
                            // Reload data for view
                            var existingP = await _context.Products.Include(p => p.ProductImages).AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
                            if (existingP != null)
                            {
                                model.CurrentMainImagePath = existingP.MainImagePath;
                                model.CurrentGalleryImages = existingP.ProductImages.ToList();
                            }
                            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "NameAr", model.CategoryId);
                            return View(model);
                        }
                    }

                    // إضافة صور معرض جديدة
                    if (model.GalleryFiles != null && model.GalleryFiles.Count > 0)
                    {
                        try
                        {
                            foreach (var file in model.GalleryFiles)
                            {
                                var path = await _fileService.SaveFileAsync(file, Constants.Folders.ProductsGallery);
                                _context.ProductImages.Add(new ProductImage { ProductId = product.Id, ImagePath = path });
                            }
                        }
                        catch (ArgumentException ex)
                        {
                             _logger.LogError(ex, "Error uploading gallery images in Edit Product");
                             TempData["ErrorMessage"] = $"تم تحديث المنتج ولكن فشل رفع بعض الصور: {ex.Message}";
                        }
                    }

                    _context.Update(product);
                    await _context.SaveChangesAsync();

                    // Evict caches
                    await _cacheStore.EvictByTagAsync("products_data", CancellationToken.None);
                    await _cacheStore.EvictByTagAsync("home_data", CancellationToken.None);

                    TempData["SuccessMessage"] = "تم تحديث المنتج بنجاح";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in {ActionName}", nameof(Edit));
                return Redirect("/Admin/Error/General");
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.ProductImages)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (product != null)
                {
                    // حذف الصورة الرئيسية
                    _fileService.DeleteFile(product.MainImagePath, Constants.Folders.Products);

                    // حذف صور المعرض
                    foreach (var img in product.ProductImages)
                    {
                        _fileService.DeleteFile(img.ImagePath, Constants.Folders.ProductsGallery);
                    }

                    _context.Products.Remove(product);
                    await _context.SaveChangesAsync();
                    
                    // Evict caches
                    await _cacheStore.EvictByTagAsync("products_data", CancellationToken.None);
                    await _cacheStore.EvictByTagAsync("home_data", CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in {ActionName}", nameof(Delete));
                return Redirect("/Admin/Error/General");
            }
            return RedirectToAction(nameof(Index));
        }
    }
}