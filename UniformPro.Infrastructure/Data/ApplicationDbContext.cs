using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UniformPro.Core.Entities;

namespace UniformPro.Infrastructure.Data
{
    // نستخدم IdentityDbContext لدعم جداول المستخدمين (Admin)
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Portfolio> Portfolios { get; set; }
        public DbSet<PortfolioMedia> PortfolioMedias { get; set; }
        public DbSet<ContactMessage> ContactMessages { get; set; }
        public DbSet<SiteSettings> SiteSettings { get; set; }
        public DbSet<HeroItem> HeroItems { get; set; }
        public DbSet<Testimonial> Testimonials { get; set; }
        public DbSet<DiscountTier> DiscountTiers { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // تخصيص دقة الأسعار لتجنب مشاكل SQL
            builder.Entity<Product>()
                .Property(p => p.StartPrice)
                .HasPrecision(18, 2);

            // تخصيص دقة نسبة الخصم
            builder.Entity<DiscountTier>()
                .Property(d => d.DiscountPercentage)
                .HasPrecision(5, 2);
        }
    }
}