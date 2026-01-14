/* ========================================
   HOME PAGE JAVASCRIPT
   Swiper.js Integration for All Sliders
   ======================================== */

// ==================== UTILITY FUNCTIONS ====================

/**
 * Validates and sanitizes YouTube URL to prevent XSS
 * @param {string} url - The YouTube URL to validate
 * @returns {string|null} - Sanitized embed URL or null if invalid
 */
function validateYouTubeUrl(url) {
    // Security: Only allow YouTube embed URLs
    if (!url || typeof url !== 'string') {
        console.error('Invalid YouTube URL provided');
        return null;
    }

    // Ensure it's a valid YouTube embed URL
    const embedPattern = /^https:\/\/www\.youtube\.com\/embed\/[a-zA-Z0-9_-]+$/;
    if (!embedPattern.test(url)) {
        console.error('YouTube URL does not match expected format:', url);
        return null;
    }

    return url;
}

/**
 * Plays YouTube video in card by replacing thumbnail with iframe
 * @param {HTMLElement} el - The element to replace with iframe
 * @param {string} url - The YouTube embed URL
 */
function playCardYoutube(el, url) {
    // Prevent action if user is dragging
    const slider = el.closest('.swiper-container');
    if (slider && slider.classList.contains('swiper-container-dragging')) {
        return;
    }

    // Validate URL for security
    const sanitizedUrl = validateYouTubeUrl(url);
    if (!sanitizedUrl) {
        return;
    }

    // Create iframe element safely
    const iframe = document.createElement('iframe');
    iframe.src = sanitizedUrl + '?autoplay=1';
    iframe.className = 'w-full h-full';
    iframe.frameBorder = '0';
    iframe.allow = 'autoplay; encrypted-media';
    iframe.allowFullscreen = true;

    // Replace content with iframe
    el.innerHTML = '';
    el.appendChild(iframe);
}

/**
 * Plays local video by removing cover and showing controls
 * @param {HTMLElement} container - The video container element
 */
function playLocalVideo(container) {
    // Prevent action if user is dragging slider
    const slider = container.closest('.swiper-container');
    if (slider && slider.dataset.isDragging === 'true') {
        return;
    }

    const video = container.querySelector('video');
    const cover = container.querySelector('.cover-img');
    const overlay = container.querySelector('.play-overlay');

    if (video) {
        // Hide cover and overlay
        if (cover) {
            cover.classList.add('opacity-0', 'pointer-events-none');
        }
        if (overlay) {
            overlay.classList.add('opacity-0', 'hidden');
        }

        // Show controls and play
        video.controls = true;
        video.play().catch(err => {
            console.error('Error playing video:', err);
        });

        // Remove click handler to prevent re-triggering
        container.onclick = null;
    }
}

// Make functions globally available
window.playCardYoutube = playCardYoutube;
window.playLocalVideo = playLocalVideo;

// ==================== HERO CAROUSEL ====================

/**
 * Initialize Hero Carousel with Swiper
 * Supports autoplay, pagination, and touch gestures with RTL
 */
function initHeroCarousel() {
    const heroElement = document.getElementById('heroCarousel');
    if (!heroElement) return;

    const isRTL = document.documentElement.dir === 'rtl';

    new Swiper('#heroCarousel', {
        direction: 'horizontal',
        loop: true,
        autoplay: {
            delay: 5000,
            disableOnInteraction: false,
            pauseOnMouseEnter: true,
        },
        speed: 600,
        effect: 'fade',
        fadeEffect: {
            crossFade: true
        },
        pagination: {
            el: '.swiper-pagination',
            clickable: true,
            bulletClass: 'hero-dot',
            bulletActiveClass: 'active',
        },
        keyboard: {
            enabled: true,
            onlyInViewport: true,
        },
        a11y: {
            enabled: true,
            prevSlideMessage: 'Previous slide',
            nextSlideMessage: 'Next slide',
            firstSlideMessage: 'This is the first slide',
            lastSlideMessage: 'This is the last slide',
        },
        rtl: isRTL,
        touchEventsTarget: 'wrapper',
        allowTouchMove: true,
    });
}

// ==================== LATEST PRODUCTS SLIDER ====================

/**
 * Initialize Latest Products Slider
 * 4 items on desktop, 2 on mobile
 */
function initProductsSlider() {
    const productsElement = document.getElementById('latestProductsSlider');
    if (!productsElement) return;

    const isRTL = document.documentElement.dir === 'rtl';

    new Swiper('#latestProductsSlider', {
        slidesPerView: 2.2,
        spaceBetween: 10,
        grabCursor: true,
        breakpoints: {
            768: {
                slidesPerView: 4,
                spaceBetween: 40,
            },
        },
        navigation: {
            nextEl: '.products-button-next',
            prevEl: '.products-button-prev',
        },
        pagination: {
            el: '.swiper-pagination',
            clickable: true,
        },
        keyboard: {
            enabled: true,
        },
        a11y: {
            enabled: true,
        },
        rtl: isRTL,
    });
}

// ==================== PORTFOLIO SLIDER ====================

/**
 * Initialize Portfolio Slider
 * 4 items on desktop, 2 on mobile
 */
function initPortfolioSlider() {
    const portfolioElement = document.getElementById('portfolioSlider');
    if (!portfolioElement) return;

    const isRTL = document.documentElement.dir === 'rtl';

    new Swiper('#portfolioSlider', {
        slidesPerView: 2.2,
        spaceBetween: 10,
        grabCursor: true,
        breakpoints: {
            768: {
                slidesPerView: 4,
                spaceBetween: 40,
            },
        },
        navigation: {
            nextEl: '.portfolio-button-next',
            prevEl: '.portfolio-button-prev',
        },
        pagination: {
            el: '.swiper-pagination',
            clickable: true,
        },
        keyboard: {
            enabled: true,
        },
        a11y: {
            enabled: true,
        },
        rtl: isRTL,
    });
}

// ==================== HAPPY CUSTOMERS SLIDER ====================

/**
 * Initialize Testimonials (Happy Customers) Slider
 * 3 items on desktop, 2 on mobile with autoplay
 */
function initTestimonialsSlider() {
    const testimonialsElement = document.getElementById('happyCustomersSlider');
    if (!testimonialsElement) return;

    const isRTL = document.documentElement.dir === 'rtl';

    const testimonialSwiper = new Swiper('#happyCustomersSlider', {
        slidesPerView: 2,
        spaceBetween: 16,
        grabCursor: true,
        loop: false,
        autoplay: {
            delay: 3000,
            disableOnInteraction: false,
            pauseOnMouseEnter: true,
        },
        breakpoints: {
            768: {
                slidesPerView: 3,
                spaceBetween: 16,
            },
        },
        navigation: {
            nextEl: '.testimonials-button-next',
            prevEl: '.testimonials-button-prev',
        },
        keyboard: {
            enabled: true,
        },
        a11y: {
            enabled: true,
        },
        rtl: isRTL,
        on: {
            touchStart: function () {
                this.dataset.isDragging = 'true';
            },
            touchEnd: function () {
                setTimeout(() => {
                    this.dataset.isDragging = 'false';
                }, 100);
            },
        },
    });

    // Track dragging state for video play prevention
    const sliderEl = document.getElementById('happyCustomersSlider');
    if (sliderEl) {
        sliderEl.dataset.isDragging = 'false';
    }
}

// ==================== FEATURES SLIDER (MOBILE ONLY) ====================

/**
 * Initialize Features Slider for mobile
 * Shows as grid on desktop
 */
function initFeaturesSlider() {
    const featuresElement = document.getElementById('featuresSlider');
    if (!featuresElement) return;

    const isRTL = document.documentElement.dir === 'rtl';

    new Swiper('#featuresSlider', {
        slidesPerView: 1,
        spaceBetween: 16,
        centeredSlides: true,
        grabCursor: true,
        pagination: {
            el: '#featuresPagination',
            clickable: true,
        },
        keyboard: {
            enabled: true,
        },
        a11y: {
            enabled: true,
        },
        rtl: isRTL,
        breakpoints: {
            768: {
                enabled: false, // Disable on desktop (use grid instead)
            },
        },
    });
}

// ==================== INITIALIZATION ====================

/**
 * Initialize all sliders on DOM ready
 */
document.addEventListener('DOMContentLoaded', function () {
    // Initialize all Swiper instances
    initHeroCarousel();
    initProductsSlider();
    initPortfolioSlider();
    initTestimonialsSlider();
    initFeaturesSlider();

    console.log('✅ Home page sliders initialized successfully');
});

// ==================== MARQUEE (Clients) ====================
// Note: Marquee uses pure CSS animation, no JS needed
// See home.css for .marquee-track animation
