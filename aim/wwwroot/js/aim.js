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

// ── Micro-animations ──────────────────────────────────────────

// 1. Animate score counter on Dashboard
function animateCounter(el) {
    const target = parseInt(el.dataset.target, 10);
    if (isNaN(target)) return;
    let current = 0;
    const step = Math.ceil(target / 40);
    const timer = setInterval(() => {
        current = Math.min(current + step, target);
        el.textContent = current.toLocaleString();
        if (current >= target) clearInterval(timer);
    }, 20);
}
document.querySelectorAll('[data-counter]').forEach(animateCounter);

// 2. Progress bars fill on load
document.querySelectorAll('.progress-fill, .badge-prog-fill, .pg-progress-fill').forEach(bar => {
    const target = bar.style.width;
    bar.style.width = '0%';
    requestAnimationFrame(() => {
        requestAnimationFrame(() => { bar.style.width = target; });
    });
});

// 3. Badge unlock pop animation (triggered by toast type=badge)
const toast = document.getElementById('aim-toast');
if (toast && toast.dataset.type === 'badge') {
    toast.classList.add('badge-pop');
}

// 4. Complete button loading state
document.querySelectorAll('form[data-complete]').forEach(form => {
    form.addEventListener('submit', () => {
        const btn = form.querySelector('button[type=submit]');
        if (btn && !btn.disabled) {
            btn.disabled = true;
            btn.textContent = '...';
        }
    });
});