function showDialog(dialog) {
    if (!(dialog instanceof HTMLDialogElement) || dialog.open) {
        return;
    }

    dialog.showModal();
}

function closeDialog(dialog) {
    if (!(dialog instanceof HTMLDialogElement) || !dialog.open) {
        return;
    }

    dialog.close();
}

document.addEventListener("click", (event) => {
    const dialog = event.target;
    if (dialog instanceof HTMLDialogElement && dialog.classList.contains("app-dialog")) {
        dialog.close();
    }
});

window.frameworkUi = window.frameworkUi ?? {};
window.frameworkUi.showDialog = showDialog;
window.frameworkUi.closeDialog = closeDialog;
