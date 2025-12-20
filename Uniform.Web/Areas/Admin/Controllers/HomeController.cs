using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniformPro.Infrastructure.Data;
using UniformPro.Web.ViewModels;

namespace UniformPro.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var model = new DashboardViewModel
            {
                // 1. حساب الأرقام الإجمالية
                ProductsCount = await _context.Products.CountAsync(),
                ProjectsCount = await _context.Portfolios.CountAsync(),
                CategoriesCount = await _context.Categories.CountAsync(),

                // 2. عدد الرسائل غير المقروءة (المهمة للمدير)
                UnreadMessagesCount = await _context.ContactMessages.CountAsync(m => !m.IsRead),

                // 3. جلب آخر 5 رسائل (الأحدث أولاً)
                RecentMessages = await _context.ContactMessages
                                               .OrderByDescending(m => m.SentAt)
                                               .Take(5)
                                               .ToListAsync(),

                // 4. جلب أحدث 5 منتجات مضافة (مع القسم الخاص بها)
                RecentProducts = await _context.Products
                                               .Include(p => p.Category)
                                               .OrderByDescending(p => p.CreatedAt)
                                               .Take(5)
                                               .ToListAsync()
            };

            return View(model);
        }
    }
}