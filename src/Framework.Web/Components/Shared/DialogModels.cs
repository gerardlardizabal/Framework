namespace Framework.Web.Components.Shared;

public enum DialogKind
{
    Default,
    Brand,
    Success,
    Warning,
    Error
}

public enum DialogMode
{
    Message,
    Confirmation
}

public sealed class DialogRequest
{
    public required string Title { get; init; }
    public string? Message { get; init; }
    public DialogMode Mode { get; init; } = DialogMode.Message;
    public DialogKind Kind { get; init; } = DialogKind.Default;
    public string AcknowledgeText { get; init; } = "OK";
    public string ConfirmText { get; init; } = "Confirm";
    public string CancelText { get; init; } = "Cancel";
    public bool Destructive { get; init; }
}
