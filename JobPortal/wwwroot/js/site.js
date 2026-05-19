/**
 * site.js — JobPortal client-side scripts
 * Handles: Bootstrap form validation, auto-dismiss alerts, active nav links
 */

// ── Bootstrap Client-Side Validation ─────────────────────────────────────────
// Adds was-validated class on submit to trigger Bootstrap validation styles
(function () {
    'use strict';
    const forms = document.querySelectorAll('.needs-validation');
    Array.from(forms).forEach(function (form) {
        form.addEventListener('submit', function (event) {
            if (!form.checkValidity()) {
                event.preventDefault();
                event.stopPropagation();
            }
            form.classList.add('was-validated');
        }, false);
    });
})();

// ── Auto-Dismiss Alerts ───────────────────────────────────────────────────────
// TempData success/error alerts fade out automatically after 4 seconds
document.addEventListener('DOMContentLoaded', function () {
    const alerts = document.querySelectorAll('.alert.alert-success, .alert.alert-danger');
    alerts.forEach(function (alert) {
        setTimeout(function () {
            const bsAlert = bootstrap.Alert.getOrCreateInstance(alert);
            if (bsAlert) bsAlert.close();
        }, 4000);
    });
});

// ── Active Nav Link Highlight ─────────────────────────────────────────────────
// Adds 'active' class to the navbar link matching the current URL path
document.addEventListener('DOMContentLoaded', function () {
    const path = window.location.pathname.toLowerCase();
    document.querySelectorAll('.site-navbar .nav-link').forEach(function (link) {
        const href = link.getAttribute('href');
        if (href && href !== '/' && path.startsWith(href.toLowerCase())) {
            link.classList.add('active');
            link.style.color = 'var(--text-primary)';
        }
    });
});
