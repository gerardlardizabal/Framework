using Framework.Application.Notifications;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Framework.Web.Notifications;

public static class NotificationEndpoints
{
    public static IEndpointConventionBuilder MapNotificationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/notifications").RequireAuthorization();

        group.MapGet("/", async Task<Ok<IReadOnlyList<NotificationDto>>> (
            IMediator mediator,
            [FromQuery] int take = 8) =>
        {
            var items = await mediator.Send(new GetNotificationsQuery(take));
            return TypedResults.Ok(items);
        });

        group.MapGet("/unread-count", async Task<Ok<UnreadCountResponse>> (IMediator mediator) =>
        {
            var count = await mediator.Send(new GetUnreadNotificationCountQuery());
            return TypedResults.Ok(new UnreadCountResponse(count));
        });

        group.MapPost("/{id:guid}/read", async Task<IResult> (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new MarkNotificationReadCommand(id));
            return result.Succeeded ? TypedResults.Ok() : TypedResults.NotFound();
        });

        group.MapPost("/read-all", async Task<Ok> (IMediator mediator) =>
        {
            await mediator.Send(new MarkAllNotificationsReadCommand());
            return TypedResults.Ok();
        });

        return group;
    }

    public sealed record UnreadCountResponse(int Count);
}
