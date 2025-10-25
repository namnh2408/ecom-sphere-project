using ShopHub.Common.CQRS;
using ShopHub.Domain.CQRS;

namespace ShopHub.Services.UserService.Application.Commands;

/// <summary>
/// Command: Logout người dùng
/// </summary>
[Transactional]
public class LogoutCommand : ICommand<bool>
{
    /// <summary>
    /// User ID
    /// </summary>
    public Guid UserId { get; set; }

    public LogoutCommand() { }

    public LogoutCommand(Guid userId)
    {
        UserId = userId;
    }
}