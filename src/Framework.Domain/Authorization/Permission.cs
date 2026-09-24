using Framework.Domain.Common;

namespace Framework.Domain.Authorization;

public sealed class Permission : AuditableEntity
{
    public string Group { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsSystem { get; set; }
}
