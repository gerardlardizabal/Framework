using Microsoft.AspNetCore.Identity;

namespace Framework.Infrastructure.Identity;

public class ApplicationRole : IdentityRole<Guid>
{
    public ApplicationRole()
    {
    }

    public ApplicationRole(string name)
        : base(name)
    {
    }

    public string? Description { get; set; }
}
