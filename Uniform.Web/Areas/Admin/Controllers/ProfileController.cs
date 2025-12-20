using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UniformPro.Web.ViewModels;

namespace UniformPro.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public ProfileController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // عرض الصفحة الرئيسية للملف الشخصي
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            ViewBag.Email = user.Email;
            ViewBag.Username = user.UserName;

            return View(new ChangePasswordViewModel());
        }

        // معالجة تغيير كلمة المرور
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            if (!ModelState.IsValid)
            {
                // إعادة البيانات للعرض في حالة الخطأ
                ViewBag.Email = user.Email;
                ViewBag.Username = user.UserName;
                return View("Index", model);
            }

            // محاولة تغيير الباسورد
            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (result.Succeeded)
            {
                // مهم جداً: إعادة تسجيل الدخول لكي لا يخرج المستخدم
                await _signInManager.RefreshSignInAsync(user);

                TempData["SuccessMessage"] = "تم تغيير كلمة المرور بنجاح!";
                return RedirectToAction(nameof(Index));
            }

            // في حالة وجود أخطاء (مثلاً الباسورد القديم خطأ)
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            ViewBag.Email = user.Email;
            ViewBag.Username = user.UserName;
            return View("Index", model);
        }
    }
}