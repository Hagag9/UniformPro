using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using UniformPro.Core.Entities;
using UniformPro.Infrastructure.Data;
using Serilog;
using Microsoft.AspNetCore.Http.Features;


var builder = WebApplication.CreateBuilder(args);

// Serilog Configuration
builder.Host.UseSerilog((context, configuration) => 
    configuration.ReadFrom.Configuration(context.Configuration));

// Increase Multipart Body Length Limit to 30MB
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 31457280; // 30 MB
});


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
builder.Services.AddHttpContextAccessor(); // Required for CatalogController

// --- Caching & Performance Services ---
builder.Services.AddMemoryCache(); // 1. Memory Cache

builder.Services.AddResponseCompression(options => // 2. Response Compression
{
    options.EnableForHttps = true;
});

builder.Services.AddOutputCache(options => // 3. Output Cache
{
    // Base Policy: Default 1 min
    options.AddBasePolicy(builder => builder.Expire(TimeSpan.FromMinutes(1)));

    // HomePage Policy: 1 hour, Tag "home_data", Vary by Culture
    options.AddPolicy("HomePage", builder => 
        builder.Expire(TimeSpan.FromHours(1))
               .Tag("home_data")
               .VaryByValue(context => new KeyValuePair<string, string>("culture", context.Features.Get<IRequestCultureFeature>()?.RequestCulture.UICulture.Name ?? "ar")));

    // Products Policy: 5 mins, Vary by query keys & Culture, Tag "products_data"
    options.AddPolicy("Products", builder => 
        builder.Expire(TimeSpan.FromMinutes(5))
               .SetVaryByQuery("category", "search", "page")
               .Tag("products_data")
               .VaryByValue(context => new KeyValuePair<string, string>("culture", context.Features.Get<IRequestCultureFeature>()?.RequestCulture.UICulture.Name ?? "ar")));

    // Portfolios Policy: 30 mins, Tag "portfolio_data", Vary by Culture
    options.AddPolicy("Portfolios", builder => 
        builder.Expire(TimeSpan.FromMinutes(30))
               .Tag("portfolio_data")
               .VaryByValue(context => new KeyValuePair<string, string>("culture", context.Features.Get<IRequestCultureFeature>()?.RequestCulture.UICulture.Name ?? "ar")));

    // ProductDetails Policy: 5 mins, Tag "products_data", Vary by Id & Culture
    options.AddPolicy("ProductDetails", builder => 
        builder.Expire(TimeSpan.FromMinutes(5))
               .Tag("products_data") // So it clears when product is edited
               .SetVaryByRouteValue("id")
               .VaryByValue(context => new KeyValuePair<string, string>("culture", context.Features.Get<IRequestCultureFeature>()?.RequestCulture.UICulture.Name ?? "ar")));

    // PortfolioDetails Policy: 5 mins, Tag "portfolio_data", Vary by Id & Culture
    options.AddPolicy("PortfolioDetails", builder => 
        builder.Expire(TimeSpan.FromMinutes(5))
               .Tag("portfolio_data") 
               .SetVaryByRouteValue("id")
               .VaryByValue(context => new KeyValuePair<string, string>("culture", context.Features.Get<IRequestCultureFeature>()?.RequestCulture.UICulture.Name ?? "ar")));

    // AboutPage Policy: 1 hour, Tag "about_data", Vary by Culture
    options.AddPolicy("AboutPage", builder => 
        builder.Expire(TimeSpan.FromHours(1))
               .Tag("about_data")
               .VaryByValue(context => new KeyValuePair<string, string>("culture", context.Features.Get<IRequestCultureFeature>()?.RequestCulture.UICulture.Name ?? "ar")));
});
// --------------------------------------
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
    var request = context.HttpContext.Request;
    var path = request.Path.Value ?? "";
    
    if (response.StatusCode == 404)
    {
        // 🔥 الإضافة الجديدة: تسجيل اللوج قبل التوجيه
        
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
        
        // بنسجلها كـ Warning   وبنسجل الرابط اللي اليوزر كتبه غلط
        logger.LogWarning("⚠️ 404 Not Found: User tried to access '{Path}' but it does not exist.", path);

        // كود التوجيه القديم زي ما هو
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
app.UseHttpsRedirection();

// --- Static Files with Aggressive Caching (1 Year) ---
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append(
             "Cache-Control", "public,max-age=31536000");
    }
});
// ----------------------------------------------------

app.UseSerilogRequestLogging();

app.UseRouting();

// 5. تفعيل تعدد اللغات
app.UseRequestLocalization();

// --- Performance Middlewares ---
app.UseResponseCompression(); // Must be before OutputCache
app.UseOutputCache();
// ------------------------------
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


// === Data Seeding ===
using (var scope = app.Services.CreateScope())
{
    try
    {
        await DbInitializer.InitializeAsync(scope.ServiceProvider);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during data seeding.");
    }
}

app.Run();