using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using TmaAuth;

namespace Example.Hubs;

public interface IBalanceNotificationService
{
}


[Authorize(AuthenticationSchemes = TmaTokenDefaults.AuthenticationScheme)]
public class BalanceHub : Hub<IBalanceNotificationService>, IBalanceNotificationService
{
    private readonly ILogger<BalanceHub> _logger;
    private readonly IHubContext<BalanceHub, IBalanceNotificationService> _context;

    public BalanceHub(ILogger<BalanceHub> logger, IHubContext<BalanceHub, IBalanceNotificationService> context)
    {
        _logger = logger;
        _context = context;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User.Identity.Name;
        string groupName = !string.IsNullOrEmpty(userId) ? userId : "default";

        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation($"BalanceHub: user {groupName} connected with connection id {Context.User.Identity.Name}");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var userId = Context.User.Identity.Name;
        string groupName = !string.IsNullOrEmpty(userId) ? userId : "default";

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation($"BalanceHub: user {groupName} disconnected with connection id {Context.User.Identity.Name}");

        await base.OnDisconnectedAsync(exception);
    }
}
