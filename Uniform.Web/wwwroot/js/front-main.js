// Initialize AOS
AOS.init({ duration: 800, once: true, offset: 100 });

// Toggle Mobile Menu
function toggleMobileMenu() {
    const menu = document.getElementById('mobile-menu');
    const overlay = document.getElementById('mobile-menu-overlay');
    const body = document.body;

    menu.classList.toggle('open');
    overlay.classList.toggle('hidden');

    if (menu.classList.contains('open')) {
        body.classList.add('overflow-hidden');
    } else {
        body.classList.remove('overflow-hidden');
    }
}

// Toggle Submenu
function toggleSubmenu(id) {
    const el = document.getElementById(id);
    const icon = document.getElementById(id + 'Icon');
    el.classList.toggle('hidden');
    if (icon) {
        icon.style.transform = el.classList.contains('hidden') ? 'rotate(0deg)' : 'rotate(180deg)';
    }
}

// Navbar Scroll Behavior
function initNavbarBehavior() {
    let lastScrollTop = 0;
    const header = document.getElementById('mainHeader');

    function checkScroll() {
        let scrollTop = window.pageYOffset || document.documentElement.scrollTop;

        // Add/Remove background based on position
        if (scrollTop > 50) {
            header.classList.add('scrolled');
        } else {
            header.classList.remove('scrolled');
        }

        // Hide/Show on scroll logic
        if (scrollTop > lastScrollTop && scrollTop > 100) {
            // Scrolling DOWN -> Hide Navbar
            header.classList.add('-translate-y-full');
        } else {
            // Scrolling UP -> Show Navbar
            header.classList.remove('-translate-y-full');
        }

        lastScrollTop = scrollTop <= 0 ? 0 : scrollTop; // For Mobile or negative scrolling
    }

    // Run on load to set initial state
    checkScroll();

    // Run on scroll
    window.addEventListener('scroll', checkScroll, { passive: true });
}

// Initialize Navbar on load
document.addEventListener('DOMContentLoaded', initNavbarBehavior);


// ========== INQUIRY CART ==========
const InquiryCart = {
    storageKey: 'inquiryCart',

    getItems: function () {
        const data = localStorage.getItem(this.storageKey);
        return data ? JSON.parse(data) : [];
    },

    saveItems: function (items) {
        localStorage.setItem(this.storageKey, JSON.stringify(items));
        this.updateBadge();
    },

    add: function (id, name, image) {
        const items = this.getItems();
        if (!items.find(item => item.id === id)) {
            items.push({ id, name, image });
            this.saveItems(items);
            this.showToast();

            // Pixel Tracking: AddToCart
            if (typeof fbq !== 'undefined') {
                fbq('track', 'AddToCart', {
                    content_ids: [id.toString()], // Must match Product ID
                    content_type: 'product',
                    content_name: name,
                    value: 0, // Inquiry Cart has no direct price
                    currency: 'EGP'
                });
            }
        }
    },

    remove: function (id) {
        const items = this.getItems().filter(item => item.id !== id);
        this.saveItems(items);
        this.renderCart();
    },

    clear: function () {
        localStorage.removeItem(this.storageKey);
        this.updateBadge();
        this.renderCart();
    },

    updateBadge: function () {
        const badge = document.getElementById('cartBadge');
        const count = this.getItems().length;
        if (badge) {
            badge.textContent = count;
            badge.classList.toggle('hidden', count === 0);
        }
    },

    showToast: function () {
        // We need to get the localized string from a data attribute or global var
        // Since we are moving out of Razor, we'll use a placeholder or data attribute strategy later.
        // For now, hardcoding english/arabic agnostic check or using a simple string.
        // Better approach: Use data attributes on the body or a config object.
        const msg = document.body.getAttribute('data-i18n-added-to-cart') || 'Added to Cart';

        const toast = document.createElement('div');
        toast.className = 'fixed bottom-4 right-4 bg-gray-900 text-white px-4 py-2 rounded-lg shadow-lg z-50 animate-bounce';
        toast.innerHTML = `<i class="ph ph-check me-1"></i> ${msg}`;
        document.body.appendChild(toast);
        setTimeout(() => toast.remove(), 2000);
    },

    renderCart: function () {
        const container = document.getElementById('cartItems');
        const emptyState = document.getElementById('cartEmptyState');
        const actionsFooter = document.getElementById('cartActionsFooter');
        const items = this.getItems();
        // const emptyMsg = document.body.getAttribute('data-i18n-cart-empty') || 'Cart is Empty'; // No longer needed for logic

        if (items.length === 0) {
            // Empty State
            container.innerHTML = ''; // Clear items
            if (container) container.classList.add('hidden');
            if (emptyState) emptyState.classList.remove('hidden');
            if (actionsFooter) actionsFooter.classList.add('hidden');
            return;
        }

        // Filled State
        if (container) container.classList.remove('hidden');
        if (emptyState) emptyState.classList.add('hidden');
        if (actionsFooter) actionsFooter.classList.remove('hidden');

        container.innerHTML = items.map(item => `
            <div class="flex items-center gap-3 p-3 mb-2 bg-white rounded-lg shadow-sm border border-gray-100">
                <img src="/uploads/products/${item.image}" class="w-16 h-16 object-cover rounded-md" onerror="this.src='/images/placeholder.png'" />
                <div class="flex-1">
                    <div class="sidebar-header-font text-sm font-bold text-gray-900 uppercase tracking-wide">${item.name}</div>
                </div>
                <button onclick="InquiryCart.remove(${item.id})" class="text-gray-400 hover:text-red-500 p-2 transition-colors">
                    <i class="ph ph-trash text-lg"></i>
                </button>
            </div>
        `).join('');
    },

    generateWhatsAppMessage: function () {
        const items = this.getItems();
        if (items.length === 0) return;

        const intro = document.body.getAttribute('data-i18n-inquiry-msg') || 'I am interested in these products:';
        const whatsappNumber = document.body.getAttribute('data-whatsapp-number');
        const baseUrl = window.location.origin;

        let message = intro + '\n\n';
        items.forEach((item, i) => {
            message += `${i + 1}. ${item.name}\n   ${baseUrl}/Products/Details/${item.id}\n\n`;
        });

        const phone = whatsappNumber.replace(/\D/g, '');
        const url = `https://api.whatsapp.com/send?phone=${phone}&text=${encodeURIComponent(message)}`;

        // Pixel Tracking: Contact
        if (typeof fbq !== 'undefined') {
            fbq('track', 'Contact', {
                content_name: 'WhatsApp Inquiry Cart'
            });
        }

        window.open(url, '_blank');
    }
};

// Global functions exposed for onclick handlers
window.addToCart = function (id, name, image) {
    InquiryCart.add(id, name, image);
}

window.toggleInquiryCart = function () {
    const sidebar = document.getElementById('inquiryCartSidebar');
    const overlay = document.getElementById('cartOverlay');
    const body = document.body;
    const isOpen = sidebar.classList.contains('cart-open');

    if (isOpen) {
        sidebar.classList.remove('cart-open');
        overlay.classList.add('hidden');
        body.classList.remove('overflow-hidden');
    } else {
        sidebar.classList.add('cart-open');
        overlay.classList.remove('hidden');
        body.classList.add('overflow-hidden');
        InquiryCart.renderCart();
    }
}

// ========== SCROLL TO TOP ==========
window.scrollToTop = function () {
    window.scrollTo({ top: 0, behavior: 'smooth' });
}

function initScrollToTop() {
    const btn = document.getElementById('scrollTopBtn');
    if (!btn) return;

    window.addEventListener('scroll', () => {
        if (window.scrollY > 400) {
            btn.classList.remove('translate-y-20', 'opacity-0');
        } else {
            btn.classList.add('translate-y-20', 'opacity-0');
        }
    }, { passive: true });
}

// Init functionalities on page load
document.addEventListener('DOMContentLoaded', function () {
    InquiryCart.updateBadge();
    initScrollToTop();
});

// Footer Accordion
function toggleFooterLinks(listId, iconId) {
    if (window.innerWidth >= 768) return; // Disable on desktop

    const list = document.getElementById(listId);
    const icon = document.getElementById(iconId);

    if (list.classList.contains('hidden')) {
        list.classList.remove('hidden');
        if (icon) icon.style.transform = 'rotate(45deg)';
    } else {
        list.classList.add('hidden');
        if (icon) icon.style.transform = 'rotate(0deg)';
    }
}
