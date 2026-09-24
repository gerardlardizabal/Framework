using FluentValidation.TestHelper;
using Framework.Application.Identity.Users;
using Shouldly;

namespace Framework.Application.Tests;

public class CreateUserCommandValidatorTests
{
    private readonly CreateUserCommandValidator _validator = new();

    [Fact]
    public void Rejects_invalid_email_and_short_password()
    {
        var result = _validator.TestValidate(new CreateUserCommand(
            "not-an-email",
            "short",
            null,
            null,
            true,
            []));

        result.ShouldHaveValidationErrorFor(x => x.Email);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Accepts_a_valid_command()
    {
        var result = _validator.TestValidate(new CreateUserCommand(
            "ada@localhost",
            "Password1!",
            "Ada",
            "Lovelace",
            true,
            ["User"]));

        result.IsValid.ShouldBeTrue();
    }
}
