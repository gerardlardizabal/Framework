function closeAccountMenus() {
    document.querySelectorAll("[data-account-menu-panel]").forEach((panel) => {
        if (panel instanceof HTMLElement && panel.matches(":popover-open") && typeof panel.hidePopover === "function") {
            panel.hidePopover();
        }
    });
}

function positionAccountMenu(trigger, panel) {
    const rect = trigger.getBoundingClientRect();
    const menu = trigger.closest("[data-account-menu]");
    const placement = menu?.getAttribute("data-account-menu-placement") ?? "header";
    const gap = 8;
    const width = Math.max(panel.offsetWidth || 0, 240);
    const height = panel.offsetHeight || 168;
    const vw = window.innerWidth;
    const vh = window.innerHeight;

    let left;
    let top;
    const collapsed = document.documentElement.classList.contains("sidebar-collapsed")
        && window.matchMedia("(min-width: 1024px)").matches;

    if (placement === "sidebar" && collapsed) {
        left = rect.right + gap;
        top = rect.bottom - height;
    } else if (placement === "sidebar") {
        left = rect.left;
        top = rect.top - height - gap;
        if (top < gap) {
            top = rect.bottom + gap;
        }
    } else {
        left = rect.right - width;
        top = rect.bottom + gap;
    }

    left = Math.min(Math.max(gap, left), Math.max(gap, vw - width - gap));
    top = Math.min(Math.max(gap, top), Math.max(gap, vh - height - gap));

    panel.style.left = `${left}px`;
    panel.style.top = `${top}px`;
    panel.style.right = "auto";
    panel.style.bottom = "auto";
    panel.style.width = `${width}px`;
}

function onAccountMenuToggle(event) {
    const panel = event.target;
    if (!(panel instanceof HTMLElement) || !panel.hasAttribute("data-account-menu-panel")) {
        return;
    }

    const menu = panel.closest("[data-account-menu]");
    const trigger = menu?.querySelector("[data-account-menu-trigger]");
    const open = panel.matches(":popover-open");

    if (trigger instanceof HTMLElement) {
        trigger.setAttribute("aria-expanded", open ? "true" : "false");
        if (open) {
            positionAccountMenu(trigger, panel);
        }
    }
}

function repositionOpenAccountMenus() {
    document.querySelectorAll("[data-account-menu-panel]").forEach((panel) => {
        if (!(panel instanceof HTMLElement) || !panel.matches(":popover-open")) {
            return;
        }

        const menu = panel.closest("[data-account-menu]");
        const trigger = menu?.querySelector("[data-account-menu-trigger]");
        if (trigger instanceof HTMLElement) {
            positionAccountMenu(trigger, panel);
        }
    });
}

document.addEventListener("toggle", onAccountMenuToggle, true);
window.addEventListener("resize", () => {
    closeAccountMenus();
});
window.addEventListener("scroll", repositionOpenAccountMenus, true);

document.addEventListener("blazor:enhancedload", closeAccountMenus);
if (window.Blazor?.addEventListener) {
    window.Blazor.addEventListener("enhancedload", closeAccountMenus);
}

window.frameworkUi = window.frameworkUi ?? {};
window.frameworkUi.closeAccountMenus = closeAccountMenus;
