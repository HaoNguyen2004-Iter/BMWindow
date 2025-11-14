if (!window.__bm_navigation_initialized) {
    window.__bm_navigation_initialized = true;

    function initNavigation() {
        console.debug('[navigation] init');

        // Click handler for any element that carries data-target (anchors, buttons, icons...)
        document.addEventListener('click', (e) => {
            const trigger = e.target.closest('[data-target]');
            if (!trigger) return;

            const targetId = trigger.getAttribute('data-target');
            if (!targetId) return;

            // Prevent normal navigation if it's an anchor used as SPA nav
            if (trigger.tagName.toLowerCase() === 'a') e.preventDefault();

            const targetEl = document.getElementById(targetId);
            if (!targetEl) {
                console.warn('[navigation] target not found:', targetId);
                return;
            }

            // Show/activate the target content section
            document.querySelectorAll('.content-section').forEach(s => s.classList.remove('active'));
            targetEl.classList.add('active');

            // Toggle active class on nav items (prefer elements with .nav-link)
            document.querySelectorAll('.sidebar .nav-link, .nav-link[data-target]').forEach(n => n.classList.remove('active'));
            const navLink = trigger.closest('.nav-link') || (trigger.classList.contains('nav-link') ? trigger : null);
            if (navLink) navLink.classList.add('active');

            // Update breadcrumb: prefer explicit data-breadcrumb, else nav text, else title inside target
            const breadcrumb = document.getElementById('breadcrumb');
            if (breadcrumb) {
                const explicit = trigger.getAttribute('data-breadcrumb');
                const navText = (navLink && navLink.textContent) ? navLink.textContent.trim() : trigger.textContent.trim();
                const fromTarget = targetEl.querySelector('h1, .page-title, .page-header h1');
                const title = explicit || navText || (fromTarget ? fromTarget.textContent.trim() : targetId);
                breadcrumb.innerHTML = `<li class="breadcrumb-item"><a href="#">Dashboard</a></li><li class="breadcrumb-item active">${title}</li>`;
            }

            // Auto-load partials for some sections (backwards compatibility)
            try {
                if (targetId === 'category-management' && typeof window.loadCategory === 'function') {
                   
                    window.loadCategory();
                }
            } catch (e) {
                console.error('[navigation] failed to auto-load partial for', targetId, e);
            }
        });

        // Sidebar toggle
        const sidebarToggle = document.getElementById('sidebarToggle');
        const sidebarEl = document.getElementById('sidebar');
        if (sidebarToggle && sidebarEl) {
            sidebarToggle.addEventListener('click', () => sidebarEl.classList.toggle('collapsed'));
        }

        // Mobile toggle
        const mobileToggle = document.getElementById('mobileToggle');
        const mainContent = document.getElementById('mainContent');
        if (mobileToggle && mainContent) {
            mobileToggle.addEventListener('click', () => mainContent.classList.toggle('mobile-open'));
        }
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initNavigation);
    } else {
        initNavigation();
    }
}

export default {};
