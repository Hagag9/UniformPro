using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniformPro.Core.Entities;
using UniformPro.Infrastructure.Data;
using UniformPro.Web.Helpers;

namespace UniformPro.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class ContactMessagesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ContactMessagesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // عرض الرسائل (الأحدث أولاً)
        public async Task<IActionResult> Index(string searchString, string filterType, int? pageNumber)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentFilterType"] = filterType; // عشان نحتفظ بحالة الزر النشط

            var messages = _context.ContactMessages.AsQueryable();

            // 1. منطق الفلترة (جديد / مقروء)
            if (!string.IsNullOrEmpty(filterType))
            {
                if (filterType == "unread")
                {
                    messages = messages.Where(m => !m.IsRead);
                }
                else if (filterType == "read")
                {
                    messages = messages.Where(m => m.IsRead);
                }
            }

            // 2. منطق البحث
            if (!string.IsNullOrEmpty(searchString))
            {
                messages = messages.Where(m => m.FullName.Contains(searchString)
                                            || m.CompanyName.Contains(searchString)
                                            || m.Phone.Contains(searchString));
            }

            // الترتيب: الأحدث أولاً
            messages = messages.OrderByDescending(m => m.SentAt);

            // 3. التقسيم
            int pageSize = 10;
            var paginatedList = await PaginatedList<ContactMessage>.CreateAsync(messages.AsNoTracking(), pageNumber ?? 1, pageSize);

            // AJAX Request
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_MessageList", paginatedList);
            }

            return View(paginatedList);
        }

        // قراءة تفاصيل الرسالة
        public async Task<IActionResult> Details(int id)
        {
            var message = await _context.ContactMessages.FindAsync(id);
            if (message == null) return NotFound();

            // تحديث الحالة إلى "مقروءة" بمجرد الفتح
            if (!message.IsRead)
            {
                message.IsRead = true;
                await _context.SaveChangesAsync();
            }

            return View(message);
        }

        // حذف رسالة
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var message = await _context.ContactMessages.FindAsync(id);
            if (message != null)
            {
                _context.ContactMessages.Remove(message);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم حذف الرسالة بنجاح";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}