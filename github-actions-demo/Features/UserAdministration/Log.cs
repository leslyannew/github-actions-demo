namespace github_actions_demo.Features.UserAdministration;

public static partial class Log
{
    [LoggerMessage(EventId = 30, EventName = "RoleCreated", Level = LogLevel.Information, Message = "`{userName}` user created new role `{roleName}`")]
    public static partial void RoleCreated(
        this ILogger logger, string userName, string roleName);

    [LoggerMessage(EventId = 31, EventName = "RoleDeleted", Level = LogLevel.Information, Message = "`{userName}` user deleted role `{roleName}`")]
    public static partial void RoleDeleted(
        this ILogger logger, string userName, string roleName);

    [LoggerMessage(EventId = 32, EventName = "UserAdministrationError", Level = LogLevel.Error, Message = "Error occured when `{action}` for `{userName}` user with `{message}`")]
    public static partial void UserAdministrationError(
       this ILogger logger, string action, string userName, string message);
}
