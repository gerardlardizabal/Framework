using FluentValidation.TestHelper;
using Framework.Application.Notifications;
using Framework.Domain.Notifications;
using Shouldly;

namespace Framework.Application.Tests;

public class CreateNotificationCommandValidatorTests
{
    private readonly CreateNotificationCommandValidator _validator = new();

    [Fact]
    public void Rejects_empty_title_and_user()
    {
        var result = _validator.TestValidate(new CreateNotificationCommand(
            Guid.Empty,
            "",
            null));

        result.ShouldHaveValidationErrorFor(x => x.UserId);
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Accepts_a_valid_command()
    {
        var result = _validator.TestValidate(new CreateNotificationCommand(
            Guid.NewGuid(),
            "Welcome to Framework",
            "Your workspace is ready.",
            NotificationKind.Brand,
            "dashboard",
            "Open dashboard"));

        result.IsValid.ShouldBeTrue();
    }
}

public class RelativeTimeTests
{
    [Fact]
    public void Describes_recent_times()
    {
        var now = new DateTimeOffset(2026, 9, 22, 2, 0, 0, TimeSpan.Zero);
        RelativeTime.From(now.AddSeconds(-10), now).ShouldBe("Just now");
        RelativeTime.From(now.AddMinutes(-5), now).ShouldBe("5m ago");
        RelativeTime.From(now.AddHours(-3), now).ShouldBe("3h ago");
    }
}
