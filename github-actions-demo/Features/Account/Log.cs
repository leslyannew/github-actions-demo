namespace github_actions_demo.Features.Home;

public static partial class Log
{
    [LoggerMessage(EventId = 20, EventName = "AccountIndex", Level = LogLevel.Information, Message = "Sign-in page visited by `{userName}`")]
    public static partial void SigninPageVisited(
        this ILogger logger, string userName);

    [LoggerMessage(EventId = 21, EventName = "AccountIndexRedirect", Level = LogLevel.Information, Message = "`{userName}` redirected to Home page")]
    public static partial void AccountPageRedirect(
        this ILogger logger, string userName);

    [LoggerMessage(EventId = 22, EventName = "ExternalLogin", Level = LogLevel.Information, Message = "Attempted login using schema `{provider}` `{returnlUrl}`")]
    public static partial void ExternalLoginVisited(
        this ILogger logger, string provider, string? returnlUrl);

    [LoggerMessage(EventId = 23, EventName = "ExternalLoginCallbackSuccess", Level = LogLevel.Information, Message = "`{userName}` was authenticated with schema `{provider}` `{returnlUrl}`")]
    public static partial void ExternalLoginCallbackSuccess(
        this ILogger logger, string username, string provider, string? returnlUrl);

    [LoggerMessage(EventId = 23, EventName = "ExternalLoginCallbackFailure", Level = LogLevel.Information, Message = "`{userName}` failed to authenticate using schema `{provider}` `{returnlUrl}`")]
    public static partial void ExternalLoginCallbackFailure(
        this ILogger logger, string username, string provider, string? returnlUrl);

    [LoggerMessage(EventId = 24, EventName = "ExternalLoginCallbackNoClaimsFound", Level = LogLevel.Information, Message = "No claims were found for this user")]
    public static partial void ExternalLoginCallbackNoClaimsFound(
        this ILogger logger);

    [LoggerMessage(EventId = 25, EventName = "UserCreated", Level = LogLevel.Information, Message = "`{userName}` user was added to user table successfully")]
    public static partial void UserCreated(
       this ILogger logger, string username);

    [LoggerMessage(EventId = 26, EventName = "StartUserLookup", Level = LogLevel.Information, Message = "Starting lookup for `{userName}` user")]
    public static partial void StartUserLookup(
      this ILogger logger, string username);

    [LoggerMessage(EventId = 27, EventName = "UserLookupResultFound", Level = LogLevel.Information, Message = "Lookup for `{userName}` user was succesfull, user logged in at `{lastLoginTime}`")]
    public static partial void UserLookupResultFound(
      this ILogger logger, string username, DateTimeOffset lastLoginTime);

    [LoggerMessage(EventId = 28, EventName = "UserLookupResultNotFound", Level = LogLevel.Information, Message = "Lookup for `{userName}` user was not succesfull, user will be added to backing store")]
    public static partial void UserLookupResultNotFound(
     this ILogger logger, string username);

    [LoggerMessage(EventId = 29, EventName = "UserCreationError", Level = LogLevel.Error, Message = "An error occured while attempting to add `{userName}` user to the backing store with error `{error}` ")]
    public static partial void UserCreationError(
       this ILogger logger, string username, string error);

    [LoggerMessage(EventId = 200, EventName = "UserAddClaimsError", Level = LogLevel.Error, Message = "An error occured while attempting to add `{userName}` user claims with error `{error}` ")]
    public static partial void UserAddClaimsError(
      this ILogger logger, string username, string error);

    [LoggerMessage(EventId = 210, EventName = "StartUserAddClaim", Level = LogLevel.Information, Message = "Starting to add `{userName}` user claims")]
    public static partial void StartUserAddClaim(
      this ILogger logger, string username);

    [LoggerMessage(EventId = 211, EventName = "UserAddClaimSuccessful", Level = LogLevel.Information, Message = "Claims added to `{userName}` user")]
    public static partial void UserAddClaimSuccessful(
     this ILogger logger, string username);

    [LoggerMessage(EventId = 212, EventName = "StartUserAddLogin", Level = LogLevel.Information, Message = "Adding login for `{userName}` user")]
    public static partial void StartUserAddLogin(
    this ILogger logger, string username);

    [LoggerMessage(EventId = 213, EventName = "UserAddLoginError", Level = LogLevel.Error, Message = "An error occured while attempting to add `{userName}` user login with error `{error}` ")]
    public static partial void UserAddLoginError(
      this ILogger logger, string username, string error);

    [LoggerMessage(EventId = 214, EventName = "UserAddSidForLogout", Level = LogLevel.Information, Message = "Adding sesssion index to `{userName}` user as claim")]
    public static partial void UserAddSidForLogout(
      this ILogger logger, string username);

    [LoggerMessage(EventId = 215, EventName = "UserIsNotEnabled", Level = LogLevel.Information, Message = "`{userName}` user is not enabled")]
    public static partial void UserIsNotEnabled(
      this ILogger logger, string username);
}

