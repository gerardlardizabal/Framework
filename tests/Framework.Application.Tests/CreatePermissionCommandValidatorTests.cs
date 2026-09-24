using FluentValidation.TestHelper;
using Framework.Application.Identity.PermissionCatalog;
using Shouldly;

namespace Framework.Application.Tests;

public class CreatePermissionCommandValidatorTests
{
    private readonly CreatePermissionCommandValidator _validator = new();

    [Fact]
    public void Rejects_invalid_tokens()
    {
        var result = _validator.TestValidate(new CreatePermissionCommand(
            "reports!",
            "view all",
            null,
            null));

        result.ShouldHaveValidationErrorFor(x => x.Group);
        result.ShouldHaveValidationErrorFor(x => x.Action);
    }

    [Fact]
    public void Accepts_a_valid_command()
    {
        var result = _validator.TestValidate(new CreatePermissionCommand(
            "Reports",
            "Export",
            "Export reports",
            "Download report files"));

        result.IsValid.ShouldBeTrue();
    }
}
