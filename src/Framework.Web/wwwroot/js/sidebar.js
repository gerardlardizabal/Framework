const COLLAPSE_KEY = "framework.sidebar.collapsed";
const COLLAPSED_CLASS = "sidebar-collapsed";
const DRAWER_CLASS = "sidebar-drawer-open";

function root() {
    return document.documentElement;
}

function applyCollapsed() {
    try {
        root().classList.toggle(COLLAPSED_CLASS, localStorage.getItem(COLLAPSE_KEY) === "1");
    } catch {
        root().classList.remove(COLLAPSED_CLASS);
    }

    syncButtons();
}

function setDrawer(open) {
    root().classList.toggle(DRAWER_CLASS, open);
    syncButtons();
}

function closeDrawer() {
    setDrawer(false);
    window.frameworkUi?.closeAccountMenus?.();
}

function toggleDrawer() {
    setDrawer(!root().classList.contains(DRAWER_CLASS));
}

function toggleCollapsed() {
    const next = !root().classList.contains(COLLAPSED_CLASS);
    try {
        localStorage.setItem(COLLAPSE_KEY, next ? "1" : "0");
    } catch {
        /* ignore quota / private mode */
    }

    root().classList.toggle(COLLAPSED_CLASS, next);
    syncButtons();
    window.frameworkUi?.closeAccountMenus?.();
}

function syncButtons() {
    const collapsed = root().classList.contains(COLLAPSED_CLASS);
    const drawerOpen = root().classList.contains(DRAWER_CLASS);
    const isDesktop = window.matchMedia("(min-width: 1024px)").matches;

    document.querySelectorAll("[data-sidebar-collapse]").forEach((button) => {
        button.setAttribute("aria-expanded", collapsed ? "false" : "true");
        button.setAttribute("aria-label", collapsed ? "Expand sidebar" : "Collapse sidebar");
    });

    document.querySelectorAll("[data-sidebar-drawer]").forEach((button) => {
        button.setAttribute("aria-expanded", drawerOpen ? "true" : "false");
        button.setAttribute("aria-label", drawerOpen ? "Close menu" : "Open menu");
    });

    const sidebar = document.getElementById("app-sidebar");
    if (sidebar) {
        const accessible = isDesktop || drawerOpen;
        sidebar.toggleAttribute("inert", !accessible);
        sidebar.setAttribute("aria-hidden", accessible ? "false" : "true");
    }
}

function onDocumentClick(event) {
    const target = event.target instanceof Element ? event.target : null;
    if (!target) {
        return;
    }

    if (target.closest("[data-sidebar-collapse]")) {
        event.preventDefault();
        toggleCollapsed();
        return;
    }

    if (target.closest("[data-sidebar-drawer]")) {
        event.preventDefault();
        toggleDrawer();
        return;
    }

    if (target.closest("[data-sidebar-backdrop], [data-sidebar-close]")) {
        event.preventDefault();
        closeDrawer();
        return;
    }

    if (window.matchMedia("(max-width: 1023px)").matches && target.closest("#app-sidebar a")) {
        closeDrawer();
    }
}

function onKeyDown(event) {
    if (event.key === "Escape") {
        closeDrawer();
    }
}

function onResize() {
    if (window.matchMedia("(min-width: 1024px)").matches) {
        closeDrawer();
        return;
    }

    syncButtons();
}

function bind() {
    applyCollapsed();
}

document.addEventListener("click", onDocumentClick);
document.addEventListener("keydown", onKeyDown);
window.addEventListener("resize", onResize);

if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", bind);
} else {
    bind();
}

document.addEventListener("blazor:enhancedload", bind);

if (window.Blazor?.addEventListener) {
    window.Blazor.addEventListener("enhancedload", () => {
        closeDrawer();
        applyCollapsed();
    });
}
