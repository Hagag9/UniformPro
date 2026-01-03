using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using UniformPro.Core.Entities;
using UniformPro.Infrastructure.Data;


var builder = WebApplication.CreateBuilder(args);

// 1. جلب نص الاتصال
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// 2. تسجيل قاعدة البيانات
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// 3. تسجيل خدمات الهوية (Identity) للمستخدمين
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// تسجيل خدمة الملفات
builder.Services.AddScoped<UniformPro.Web.Services.IFileService, UniformPro.Web.Services.FileService>();
builder.Services.AddSingleton<UniformPro.Web.Services.IHtmlSanitizerService, UniformPro.Web.Services.HtmlSanitizerService>();
builder.Services.AddControllersWithViews()
    .AddViewLocalization(Microsoft.AspNetCore.Mvc.Razor.LanguageViewLocationExpanderFormat.Suffix);
builder.Services.AddRazorPages();

// 4. إعداد تعدد اللغات (Localization)
//builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddLocalization();
builder.Services.AddHostedService<UniformPro.Web.Services.FileCleanupBackgroundService>();
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { new CultureInfo("ar"), new CultureInfo("en") };
    options.DefaultRequestCulture = new RequestCulture("ar");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    
    // Use Cookie to persist language choice
    options.RequestCultureProviders.Insert(0, new CookieRequestCultureProvider());
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // app.UseDeveloperExceptionPage(); // Keep commented or remove based on preference, using middleware below
    app.UseMiddleware<UniformPro.Web.Middleware.GlobalExceptionMiddleware>();
}
else
{
    app.UseMiddleware<UniformPro.Web.Middleware.GlobalExceptionMiddleware>();
    app.UseHsts();
}

// معالجة صفحات 404 (الصفحات غير الموجودة)
// توجيه بناءً على المنطقة (Admin vs Public)
app.UseStatusCodePages(async context =>
{
    var response = context.HttpContext.Response;
    var path = context.HttpContext.Request.Path.Value ?? "";
    
    if (response.StatusCode == 404)
    {
        var isAdmin = path.StartsWith("/Admin", StringComparison.OrdinalIgnoreCase);
        if (isAdmin)
        {
            response.Redirect("/Admin/Error/NotFound");
        }
        else
        {
            response.Redirect("/Error/NotFound");
        }
    }
});

app.UseHttpsRedirection();
app.UseStaticFiles(); // للسماح بملفات wwwroot

app.UseRouting();

// 5. تفعيل تعدد اللغات
app.UseRequestLocalization();
app.UseAuthentication();
app.UseAuthorization();

// توجيه لوحة التحكم
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();


// === بداية منطقة الـ Seeding (البيانات الافتراضية) ===
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider; 

    try
    {
        // 1. إنشاء المستخدم الأدمن (Admin User)
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var adminEmail = "admin@uniformpro.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            var newAdmin = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
            await userManager.CreateAsync(newAdmin, "Admin123!");
        }

        // 2. تهيئة إعدادات الموقع الافتراضية (Site Settings)
        var context = services.GetRequiredService<ApplicationDbContext>();

        // التحقق من وجود إعدادات مسبقة
        if (!context.SiteSettings.Any())
        {
            context.SiteSettings.Add(new SiteSettings
            {
                WebsiteNameAr = "يونيفورم برو",
                WebsiteNameEn = "Uniform Pro",
                PhoneNumber = "01000000000",
                Email = "info@uniformpro.com"
            });
            await context.SaveChangesAsync(); // 👈 لا تنسَ await هنا
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "حدث خطأ أثناء تهيئة البيانات الافتراضية.");
    }
}
// === نهاية منطقة الـ Seeding ===

app.Run();