using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniformPro.Infrastructure.Data;

namespace UniformPro.Web.Controllers
{
    public class ProductsController : FrontBaseController
    {
        public ProductsController(ApplicationDbContext context) : base(context) { }

        // GET: /Products?category=1&search=shirt&sort=newest&page=1&pageSize=12
        public async Task<IActionResult> Index(int? category, string? search, string? sort, int page = 1, int pageSize = 12)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 12;

            var isArabic = ViewData["IsArabic"] as bool? ?? true;
            ViewData["Title"] = isArabic ? "المنتجات" : "Products";

            // Load categories for sidebar - Removed unnecessary Include(Products) for performance
            var categories = await _context.Categories
                .AsNoTracking()
                .Where(c => c.IsActive)
                .OrderBy(c => c.NameAr)
                .ToListAsync();

            ViewBag.Categories = categories;
            ViewBag.SelectedCategory = category;
            ViewBag.SearchTerm = search;
            ViewBag.SortOrder = sort;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;

            // Build query with AsNoTracking for performance
            var productsQuery = _context.Products
                .AsNoTracking()
                .AsQueryable();

            // Filter by category
            if (category.HasValue && category.Value > 0)
            {
                productsQuery = productsQuery.Where(p => p.CategoryId == category.Value);
                var selectedCat = categories.FirstOrDefault(c => c.Id == category.Value);
                ViewData["Title"] = isArabic ? (selectedCat?.NameAr ?? "المنتجات") : (selectedCat?.NameEn ?? "Products");
            }

            // Filter by search (Arabic or English name)
            if (!string.IsNullOrWhiteSpace(search))
            {
                productsQuery = productsQuery.Where(p => 
                    p.NameAr.Contains(search) || 
                    p.NameEn.Contains(search));
            }

            // Apply Sorting
            productsQuery = sort switch
            {
                "price_asc" => productsQuery.OrderBy(p => p.StartPrice),
                "price_desc" => productsQuery.OrderByDescending(p => p.StartPrice),
                _ => productsQuery.OrderByDescending(p => p.CreatedAt) // Default: Newest
            };

            // Get total count for pagination
            var totalProducts = await productsQuery.CountAsync();
            ViewBag.TotalProducts = totalProducts;
            ViewBag.TotalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);

            // Apply pagination and projection to ViewModel
            var products = await productsQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new UniformPro.Web.ViewModels.ProductListViewModel
                {
                    Id = p.Id,
                    NameAr = p.NameAr,
                    NameEn = p.NameEn,
                    MainImagePath = p.MainImagePath,
                    MaterialDetailsAr = p.MaterialDetailsAr,
                    MaterialDetailsEn = p.MaterialDetailsEn,
                    StartPrice = p.StartPrice
                })
                .ToListAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_ProductList", products);
            }

            return View(products);
        }

        // GET: /Products/Details/5
        // GET: /Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            // Map to ViewModel
            var viewModel = new UniformPro.Web.ViewModels.ProductDetailsViewModel
            {
                Id = product.Id,
                NameAr = product.NameAr,
                NameEn = product.NameEn,
                DescriptionAr = product.DescriptionAr,
                DescriptionEn = product.DescriptionEn,
                StartPrice = product.StartPrice,
                MaterialDetailsAr = product.MaterialDetailsAr,
                MaterialDetailsEn = product.MaterialDetailsEn,
                MinQuantity = product.MinQuantity,
                AvailableSizes = product.AvailableSizes,
                CategoryId = product.CategoryId,
                CategoryNameAr = product.Category?.NameAr ?? "",
                CategoryNameEn = product.Category?.NameEn ?? "",
                MainImagePath = product.MainImagePath,
                ProductImages = product.ProductImages.ToList(),
                MetaDescription = product.MetaDescription,
                MetaKeywords = product.MetaKeywords
            };

            ViewData["Title"] = product.NameAr;
            ViewData["MetaDescription"] = product.MetaDescription;
            ViewData["MetaKeywords"] = product.MetaKeywords;

            // Load related products (same category, max 4)
            var relatedProducts = await _context.Products
                .AsNoTracking()
                .Where(p => p.CategoryId == product.CategoryId && p.Id != product.Id)
                .Take(4)
                .ToListAsync();
            ViewBag.RelatedProducts = relatedProducts;

            return View(viewModel);
        }
    }
}
