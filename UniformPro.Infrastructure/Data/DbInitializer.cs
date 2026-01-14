using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UniformPro.Core.Entities;

namespace UniformPro.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            try
            {
                var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
                var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                var logger = serviceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

                // التأكد من إنشاء قاعدة البيانات
                // await context.Database.MigrateAsync(); // اختياري: إذا كنت تريد تطبيق الترحيلات تلقائياً

                // 1. إنشاء المستخدم الأدمن (Admin User)
                var adminEmail = configuration["AdminSettings:Email"] ?? "admin@manouniform.com";
                var adminPassword = configuration["AdminSettings:Password"] ?? "Admin123!";

                var adminUser = await userManager.FindByEmailAsync(adminEmail);
                if (adminUser == null)
                {
                    var newAdmin = new IdentityUser 
                    { 
                        UserName = adminEmail, 
                        Email = adminEmail, 
                        EmailConfirmed = true 
                    };
                    var result = await userManager.CreateAsync(newAdmin, adminPassword);
                    if (result.Succeeded)
                    {
                        logger.LogInformation("Admin user created successfully.");
                    }
                    else
                    {
                        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                        logger.LogError($"Failed to create admin user: {errors}");
                    }
                }

                // 2. تهيئة إعدادات الموقع الافتراضية (Site Settings)
                if (!await context.SiteSettings.AnyAsync())
                {
                    context.SiteSettings.Add(new SiteSettings
                    {
                        WebsiteNameAr = "يونيفورم برو",
                        WebsiteNameEn = "Uniform Pro",
                        PhoneNumber = "01000000000",
                        Email = "info@uniformpro.com"
                    });
                    await context.SaveChangesAsync();
                    logger.LogInformation("Default Site Settings created.");
                }
            }
            catch (Exception ex)
            {
                var logger = serviceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();
                logger.LogError(ex, "An error occurred while seeding the database.");
            }
        }
    }
}
