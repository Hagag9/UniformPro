using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniformPro.Infrastructure.Data;

namespace UniformPro.Web.ViewComponents
{
    public class SidebarCategoriesViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public SidebarCategoriesViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var categories = await _context.Categories
                .Where(c => c.IsActive)
                .AsNoTracking()
                .ToListAsync();

            return View(categories);
        }
    }
}
