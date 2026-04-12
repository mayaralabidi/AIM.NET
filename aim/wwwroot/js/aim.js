// Mobile menu
function toggleMenu() {
    document.getElementById('mobileMenu').classList.toggle('open');
}

// Active nav link highlight
document.addEventListener('DOMContentLoaded', function () {
    const path = window.location.pathname.toLowerCase();
    document.querySelectorAll('.nav-link, .mob-link').forEach(function (link) {
        const href = link.getAttribute('href') || '';
        if (href !== '/' && path.startsWith(href.toLowerCase())) {
            link.classList.add('active');
        }
    });
});