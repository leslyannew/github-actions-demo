namespace github_actions_demo.Features.Home;

public static partial class Log
{
    [LoggerMessage(EventId = 10, EventName = "HomeIndex", Level = LogLevel.Information, Message = "Home page visited by `{userName}`")]
    public static partial void HomePageVisited(
        this ILogger logger, string userName);

    [LoggerMessage(EventId = 11, EventName = "HomeIndexRedirect", Level = LogLevel.Information, Message = "Anonymous user redirected for sign in")]
    public static partial void HomePageRedirected(
        this ILogger logger);

    [LoggerMessage(EventId = 13, EventName = "PrivacyPage", Level = LogLevel.Information, Message = "Privacy page visited by `{userName}`")]
    public static partial void PrivacyPageVisited(
        this ILogger logger, string userName);
}

