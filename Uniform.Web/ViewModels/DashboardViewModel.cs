using UniformPro.Core.Entities;

namespace UniformPro.Web.ViewModels
{
    public class DashboardViewModel
    {
        public int ProductsCount { get; set; }
        public int ProjectsCount { get; set; }
        public int UnreadMessagesCount { get; set; }
        public int CategoriesCount { get; set; }

        // لعرض آخر 5 رسائل في الجدول
        public List<ContactMessage> RecentMessages { get; set; } = new List<ContactMessage>();

        // لعرض أحدث المنتجات (اختياري)
        public List<Product> RecentProducts { get; set; } = new List<Product>();
    }
}