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

// ── Sidebar Collapse Toggle ───────────────────────────────────────────────────
document.addEventListener("DOMContentLoaded", function() {
    const sidebar = document.getElementById('sidebarOffcanvas');
    const toggleBtn = document.getElementById('sidebar-toggle');
    const mainContent = document.querySelector('.dashboard-main');
    
    if (sidebar && toggleBtn) {
        toggleBtn.addEventListener('click', function() {
            sidebar.classList.toggle('sidebar-collapsed');
            if(mainContent) {
                mainContent.classList.toggle('sidebar-collapsed');
            }
        });
    }
});

// ── Dropdown Navigation Menu Toggle ───────────────────────────────────────────
document.addEventListener("DOMContentLoaded", function () {
    const menuToggle = document.getElementById("jp-menu-toggle");
    const dropdownNav = document.getElementById("jp-dropdown-nav");

    if (menuToggle && dropdownNav) {
        menuToggle.addEventListener("click", function (event) {
            event.stopPropagation();
            const isOpen = dropdownNav.classList.toggle("jp-dropdown-open");
            
            // Update ARIA accessibility attributes
            menuToggle.setAttribute("aria-expanded", isOpen ? "true" : "false");
            dropdownNav.setAttribute("aria-hidden", isOpen ? "false" : "true");
        });

        // Close dropdown when clicking outside
        document.addEventListener("click", function (event) {
            if (!dropdownNav.contains(event.target) && !menuToggle.contains(event.target)) {
                if (dropdownNav.classList.contains("jp-dropdown-open")) {
                    dropdownNav.classList.remove("jp-dropdown-open");
                    menuToggle.setAttribute("aria-expanded", "false");
                    dropdownNav.setAttribute("aria-hidden", "true");
                }
            }
        });

        // Close dropdown when hitting escape key
        document.addEventListener("keydown", function (event) {
            if (event.key === "Escape" && dropdownNav.classList.contains("jp-dropdown-open")) {
                dropdownNav.classList.remove("jp-dropdown-open");
                menuToggle.setAttribute("aria-expanded", "false");
                dropdownNav.setAttribute("aria-hidden", "true");
                menuToggle.focus();
            }
        });
    }
});
