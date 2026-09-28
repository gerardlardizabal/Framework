namespace Framework.Web.Components.Shared;

public sealed class DialogService : IDialogService
{
    private Func<DialogRequest, Task<bool>>? handler;

    internal void Register(Func<DialogRequest, Task<bool>> show)
    {
        handler = show;
    }

    internal void Unregister(Func<DialogRequest, Task<bool>> show)
    {
        if (handler == show)
        {
            handler = null;
        }
    }

    public Task ShowMessageAsync(
        string title,
        string? message = null,
        DialogKind kind = DialogKind.Default,
        string acknowledgeText = "OK")
        => Show(new DialogRequest
        {
            Title = title,
            Message = message,
            Mode = DialogMode.Message,
            Kind = kind,
            AcknowledgeText = acknowledgeText
        });

    public Task<bool> ConfirmAsync(
        string title,
        string? message = null,
        DialogKind kind = DialogKind.Warning,
        string confirmText = "Confirm",
        string cancelText = "Cancel",
        bool destructive = false)
        => Show(new DialogRequest
        {
            Title = title,
            Message = message,
            Mode = DialogMode.Confirmation,
            Kind = destructive && kind == DialogKind.Warning ? DialogKind.Error : kind,
            ConfirmText = confirmText,
            CancelText = cancelText,
            Destructive = destructive
        });

    private Task<bool> Show(DialogRequest request)
    {
        if (handler is null)
        {
            throw new InvalidOperationException(
                "Add <DialogHost /> to the interactive page (or layout) before showing a dialog.");
        }

        return handler(request);
    }
}
