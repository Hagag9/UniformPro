using Microsoft.AspNetCore.Mvc;
using UniformPro.Core.Entities;
using UniformPro.Infrastructure.Data;
using UniformPro.Web.ViewModels;

namespace UniformPro.Web.Controllers
{
    public class ContactController : FrontBaseController
    {
        public ContactController(ApplicationDbContext context) : base(context)
        {
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ContactViewModel model)
        {
            if (ModelState.IsValid)
            {
                var contactMessage = new ContactMessage
                {
                    FullName = Sanitize(model.FullName),
                    Phone = Sanitize(model.Phone), // Save as is (e.g. 010xxxx), Admin logic handles the "2" prefix for WhatsApp
                    Email = Sanitize(model.Email),
                    CompanyName = Sanitize(model.CompanyName),
                    Message = Sanitize(model.Message),
                    SentAt = DateTime.Now,
                    IsRead = false
                };

                _context.ContactMessages.Add(contactMessage);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Thank you! Your message has been sent successfully.";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }
        private string Sanitize(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            // Remove HTML tags using Regex
            return System.Text.RegularExpressions.Regex.Replace(input, "<.*?>", string.Empty);
        }
    }
}
