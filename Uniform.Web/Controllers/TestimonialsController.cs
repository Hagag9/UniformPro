using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniformPro.Core.Entities;
using UniformPro.Infrastructure.Data;

namespace UniformPro.Web.Controllers
{
    public class TestimonialsController : FrontBaseController
    {
        public TestimonialsController(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IActionResult> Index()
        {
            var testimonials = await _context.Testimonials
                .AsNoTracking()
                .Where(t => t.IsActive)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return View(testimonials);
        }
    }
}
