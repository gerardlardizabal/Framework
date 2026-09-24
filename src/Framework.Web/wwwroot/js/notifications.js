const MAX_TOASTS = 3;
const DEFAULT_TIMEOUT = 5000;
const POLL_MS = 10000;

function toastRegion() {
    return document.getElementById("toast-region");
}

function antiforgeryHeaders() {
    const input = document.querySelector("[data-notification-menu] input[name='__RequestVerificationToken']");
    return input?.value ? { RequestVerificationToken: input.value } : {};
}

function kindClass(kind) {
    const value = (kind ?? "default").toLowerCase();
    if (["brand", "success", "warning", "error"].includes(value)) {
        return `notification-kind-${value}`;
    }

    return "notification-kind-default";
}

function kindIcon(kind) {
    const value = (kind ?? "default").toLowerCase();
    if (value === "success") {
        return `<svg class="size-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.67" aria-hidden="true"><path stroke-linecap="round" stroke-linejoin="round" d="m5 13 4 4L19 7" /></svg>`;
    }

    if (value === "warning") {
        return `<svg class="size-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.67" aria-hidden="true"><path stroke-linecap="round" d="M12 9v4M12 17h.01" /><path stroke-linecap="round" stroke-linejoin="round" d="M10.3 4.7 2.8 18a2 2 0 0 0 1.7 3h15a2 2 0 0 0 1.7-3L13.7 4.7a2 2 0 0 0-3.4 0Z" /></svg>`;
    }

    if (value === "error") {
        return `<svg class="size-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.67" aria-hidden="true"><circle cx="12" cy="12" r="8" /><path stroke-linecap="round" d="m9 9 6 6M15 9l-6 6" /></svg>`;
    }

    if (value === "brand") {
        return `<svg class="size-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.67" aria-hidden="true"><path stroke-linecap="round" stroke-linejoin="round" d="M13 3 4 14h7l-1 7 9-11h-7z" /></svg>`;
    }

    return `<svg class="size-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.67" aria-hidden="true"><circle cx="12" cy="12" r="8" /><path stroke-linecap="round" d="M12 8v4M12 16h.01" /></svg>`;
}

function closeToast(toast) {
    if (!(toast instanceof HTMLElement) || toast.dataset.closing === "1") {
        return;
    }

    toast.dataset.closing = "1";
    toast.classList.add("is-leaving");
    window.setTimeout(() => toast.remove(), 180);
}

function showToast(options) {
    const region = toastRegion();
    if (!region) {
        return;
    }

    const kind = options?.kind ?? "default";
    const title = options?.title ?? "Notification";
    const message = options?.message ?? "";
    const timeout = Number.isFinite(options?.timeout) ? options.timeout : DEFAULT_TIMEOUT;
    const actionUrl = options?.actionUrl;
    const actionText = options?.actionText ?? "View";

    while (region.children.length >= MAX_TOASTS) {
        closeToast(region.lastElementChild);
    }

    const toast = document.createElement("div");
    toast.className = "toast";
    toast.setAttribute("role", kind === "error" ? "alert" : "status");
    toast.innerHTML = `
        <span class="notification-kind ${kindClass(kind)}">${kindIcon(kind)}</span>
        <div class="min-w-0 flex-1">
            <p class="text-sm font-semibold text-gray-900"></p>
            ${message ? `<p class="mt-1 text-sm text-gray-600"></p>` : ""}
            ${actionUrl ? `<a class="mt-3 inline-flex text-sm font-semibold text-brand-700 hover:text-brand-800" href="${actionUrl}"></a>` : ""}
        </div>
        <button type="button" class="toast-close" aria-label="Dismiss">
            <svg class="size-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.67" aria-hidden="true"><path stroke-linecap="round" d="M6 6l12 12M18 6 6 18" /></svg>
        </button>
    `;

    toast.querySelector("p.text-sm.font-semibold").textContent = title;
    const messageNode = toast.querySelector("p.text-gray-600");
    if (messageNode) {
        messageNode.textContent = message;
    }

    const actionNode = toast.querySelector("a");
    if (actionNode) {
        actionNode.textContent = actionText;
        actionNode.setAttribute("href", actionUrl);
    }

    toast.querySelector(".toast-close").addEventListener("click", () => closeToast(toast));
    region.prepend(toast);

    let timer = null;
    const startTimer = () => {
        if (timeout <= 0) {
            return;
        }

        timer = window.setTimeout(() => closeToast(toast), timeout);
    };

    const stopTimer = () => {
        if (timer) {
            window.clearTimeout(timer);
            timer = null;
        }
    };

    toast.addEventListener("mouseenter", stopTimer);
    toast.addEventListener("mouseleave", startTimer);
    toast.addEventListener("focusin", stopTimer);
    toast.addEventListener("focusout", startTimer);
    startTimer();
}

function closeNotificationMenus() {
    document.querySelectorAll("[data-notification-panel]").forEach((panel) => {
        if (panel instanceof HTMLElement && panel.matches(":popover-open") && typeof panel.hidePopover === "function") {
            panel.hidePopover();
        }
    });
}

function positionNotificationMenu(trigger, panel) {
    const rect = trigger.getBoundingClientRect();
    const gap = 8;
    const width = Math.min(384, window.innerWidth - gap * 2);
    const left = Math.min(
        Math.max(gap, rect.right - width),
        Math.max(gap, window.innerWidth - width - gap)
    );
    const top = rect.bottom + gap;

    panel.style.position = "fixed";
    panel.style.inset = "auto";
    panel.style.margin = "0";
    panel.style.width = `${width}px`;
    panel.style.left = `${left}px`;
    panel.style.top = `${top}px`;
    panel.style.right = "auto";
    panel.style.bottom = "auto";
}

function repositionOpenNotificationMenus() {
    document.querySelectorAll("[data-notification-panel]").forEach((panel) => {
        if (!(panel instanceof HTMLElement) || !panel.matches(":popover-open")) {
            return;
        }

        const menu = panel.closest("[data-notification-menu]");
        const trigger = menu?.querySelector("[data-notification-trigger]");
        if (trigger instanceof HTMLElement) {
            positionNotificationMenu(trigger, panel);
        }
    });
}

function setUnread(count) {
    const value = Math.max(0, Number(count) || 0);
    document.querySelectorAll("[data-notification-menu]").forEach((menu) => {
        const badge = menu.querySelector("[data-notification-badge]");
        const trigger = menu.querySelector("[data-notification-trigger]");
        const subtitle = menu.querySelector("[data-notification-subtitle]");
        const readAll = menu.querySelector("[data-notification-read-all]");
        if (badge) {
            badge.textContent = value > 9 ? "9+" : String(value);
            badge.classList.toggle("is-on", value > 0);
            badge.toggleAttribute("hidden", value === 0);
        }

        if (trigger) {
            trigger.setAttribute("aria-label", value === 0 ? "Notifications" : `${value} unread notifications`);
        }

        if (subtitle) {
            subtitle.textContent = value === 0 ? "No unread messages" : `${value} unread`;
        }

        if (readAll instanceof HTMLButtonElement) {
            readAll.disabled = value === 0;
        }
    });
}

function rowHtml(item) {
    const unread = item.isRead ? "" : " is-unread";
    const href = item.actionUrl || "notifications";
    const message = item.message ? `<span class="mt-0.5 block text-sm text-gray-600"></span>` : "";
    const row = document.createElement("a");
    row.className = `notification-row${unread}`;
    row.href = href;
    row.dataset.notificationId = item.id;
    row.dataset.notificationRead = item.isRead ? "true" : "false";
    row.innerHTML = `
        <span class="notification-kind ${kindClass(item.kind)}">${kindIcon(item.kind)}</span>
        <span class="min-w-0 flex-1">
            <span class="flex items-start justify-between gap-3">
                <span class="text-sm font-semibold text-gray-900"></span>
                <span class="shrink-0 text-xs text-gray-500"></span>
            </span>
            ${message}
        </span>
    `;
    row.querySelector(".text-sm.font-semibold").textContent = item.title;
    row.querySelector(".text-xs").textContent = item.relativeTime;
    const messageNode = row.querySelector(".text-gray-600");
    if (messageNode && item.message) {
        messageNode.textContent = item.message;
    }

    return row;
}

function renderList(items) {
    document.querySelectorAll("[data-notification-list]").forEach((list) => {
        list.replaceChildren();
        if (!items.length) {
            list.innerHTML = `
                <div class="notification-empty">
                    <span class="notification-kind notification-kind-success">${kindIcon("success")}</span>
                    <p class="mt-3 text-sm font-semibold text-gray-900">You're all caught up</p>
                    <p class="mt-1 text-sm text-gray-600">New notifications will appear here.</p>
                </div>`;
            return;
        }

        items.forEach((item) => list.append(rowHtml(item)));
    });
}

function renderError() {
    document.querySelectorAll("[data-notification-list]").forEach((list) => {
        list.innerHTML = `
            <div class="notification-empty">
                <p class="text-sm font-semibold text-gray-900">Couldn't load notifications</p>
                <p class="mt-1 text-sm text-gray-600">Open the inbox page or try again in a moment.</p>
            </div>`;
    });
    document.querySelectorAll("[data-notification-subtitle]").forEach((subtitle) => {
        subtitle.textContent = "Unavailable";
    });
}

function asItems(payload) {
    if (Array.isArray(payload)) {
        return payload;
    }

    if (Array.isArray(payload?.items)) {
        return payload.items;
    }

    return [];
}

function asUnread(payload) {
    if (typeof payload === "number") {
        return payload;
    }

    const value = payload?.count ?? payload?.Count;
    return Number.isFinite(value) ? value : 0;
}

function inboxNeedsHydration() {
    const subtitle = document.querySelector("[data-notification-subtitle]");
    return subtitle?.textContent?.trim() === "Loading…";
}

async function fetchJson(url, options = {}) {
    const { headers, ...rest } = options;
    const response = await fetch(url, {
        credentials: "same-origin",
        headers: {
            Accept: "application/json",
            ...antiforgeryHeaders(),
            ...headers
        },
        ...rest
    });

    if (!response.ok) {
        throw new Error(`Request failed: ${response.status}`);
    }

    if (response.status === 204) {
        return null;
    }

    const contentType = response.headers.get("content-type") ?? "";
    return contentType.includes("application/json") ? response.json() : null;
}

let inflight = null;

async function refreshInbox() {
    if (!document.querySelector("[data-notification-menu]")) {
        return;
    }

    if (inflight) {
        return inflight;
    }

    inflight = (async () => {
        try {
            const [items, unread] = await Promise.all([
                fetchJson("/api/notifications?take=8"),
                fetchJson("/api/notifications/unread-count")
            ]);
            renderList(asItems(items));
            setUnread(asUnread(unread));
        } catch {
            renderError();
        }
    })().finally(() => {
        inflight = null;
    });

    return inflight;
}

async function markRead(id) {
    await fetchJson(`/api/notifications/${id}/read`, { method: "POST" });
}

async function markAllRead() {
    await fetchJson("/api/notifications/read-all", { method: "POST" });
    await refreshInbox();
}

function isOpenState(event, panel) {
    if (typeof event.newState === "string") {
        return event.newState === "open";
    }

    return panel.matches(":popover-open");
}

function onNotificationToggle(event) {
    const panel = event.target;
    if (!(panel instanceof HTMLElement) || !panel.hasAttribute("data-notification-panel")) {
        return;
    }

    const menu = panel.closest("[data-notification-menu]");
    const trigger = menu?.querySelector("[data-notification-trigger]");
    const open = isOpenState(event, panel);
    if (trigger instanceof HTMLElement) {
        trigger.setAttribute("aria-expanded", open ? "true" : "false");
        if (open) {
            requestAnimationFrame(() => {
                positionNotificationMenu(trigger, panel);
                refreshInbox();
            });
        }
    }
}

function onDocumentClick(event) {
    const target = event.target instanceof Element ? event.target : null;
    if (!target) {
        return;
    }

    if (target.closest("[data-notification-trigger]")) {
        refreshInbox();
        return;
    }

    if (target.closest("[data-notification-read-all]")) {
        event.preventDefault();
        markAllRead();
        return;
    }

    const demo = target.closest("[data-toast-kind]");
    if (demo instanceof HTMLElement) {
        event.preventDefault();
        showToast({
            kind: demo.dataset.toastKind,
            title: demo.dataset.toastTitle,
            message: demo.dataset.toastMessage,
            actionUrl: demo.dataset.toastActionUrl,
            actionText: demo.dataset.toastActionText
        });
        return;
    }

    const row = target.closest("[data-notification-id]");
    if (row instanceof HTMLAnchorElement && row.dataset.notificationRead !== "true") {
        markRead(row.dataset.notificationId)
            .then(() => refreshInbox())
            .catch(() => { /* navigation still proceeds */ });
    }
}

function hydrateIfNeeded() {
    if (inboxNeedsHydration()) {
        refreshInbox();
    }
}

document.addEventListener("toggle", onNotificationToggle, true);
document.addEventListener("click", onDocumentClick);
document.addEventListener("keydown", (event) => {
    if (event.key === "Escape") {
        closeNotificationMenus();
    }
});
window.addEventListener("resize", repositionOpenNotificationMenus);
window.addEventListener("scroll", repositionOpenNotificationMenus, true);
document.addEventListener("visibilitychange", () => {
    if (!document.hidden) {
        refreshInbox();
    }
});
document.addEventListener("blazor:enhancedload", () => {
    closeNotificationMenus();
    refreshInbox();
});
if (window.Blazor?.addEventListener) {
    window.Blazor.addEventListener("enhancedload", () => {
        closeNotificationMenus();
        refreshInbox();
    });
}

new MutationObserver(() => hydrateIfNeeded()).observe(document.documentElement, {
    childList: true,
    subtree: true
});

window.setInterval(() => {
    if (document.querySelector("[data-notification-menu]")) {
        refreshInbox();
    }
}, POLL_MS);

window.frameworkUi = window.frameworkUi ?? {};
window.frameworkUi.toast = showToast;
window.frameworkUi.closeNotificationMenus = closeNotificationMenus;
window.frameworkUi.refreshNotifications = refreshInbox;

if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", refreshInbox);
} else {
    refreshInbox();
}
