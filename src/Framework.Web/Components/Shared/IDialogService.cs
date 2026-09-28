namespace Framework.Web.Components.Shared;

public interface IDialogService
{
    Task ShowMessageAsync(
        string title,
        string? message = null,
        DialogKind kind = DialogKind.Default,
        string acknowledgeText = "OK");

    Task<bool> ConfirmAsync(
        string title,
        string? message = null,
        DialogKind kind = DialogKind.Warning,
        string confirmText = "Confirm",
        string cancelText = "Cancel",
        bool destructive = false);
}
