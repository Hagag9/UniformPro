using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniformPro.Infrastructure.Data;
using UniformPro.Web.ViewModels;
using Microsoft.AspNetCore.Identity;
namespace UniformPro.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly SignInManager<IdentityUser> _signInManager;

        public HomeController(ApplicationDbContext context,SignInManager<IdentityUser> signInManager)
        {
            _context = context;
            _signInManager = signInManager;
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
        //   دالة تسجيل الخروج
        [HttpGet] // تعمل عند الضغط على الرابط مباشرة
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            // توجيه المستخدم لصفحة الدخول بعد الخروج
            return Redirect("/Identity/Account/Login");
        }
    }
}