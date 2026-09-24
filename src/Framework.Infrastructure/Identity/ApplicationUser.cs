using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Framework.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool IsActive { get; set; } = true;

    [NotMapped]
    public string DisplayName
    {
        get
        {
            var name = $"{FirstName} {LastName}".Trim();
            return string.IsNullOrWhiteSpace(name) ? Email ?? UserName ?? "User" : name;
        }
    }
}
