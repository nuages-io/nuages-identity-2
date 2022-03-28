namespace Nuages.Identity.Services.SecurityLog;

public class SecurityLog
{
    public string Id { get; set; }
    public DateTime DateAndTime { get; set; }
    public SecurityLogType Type { get; set; }
    public string UserId { get; set; }
    
}

public enum SecurityLogType
{
    Login,
    TwoFactorChallengeSuccess,
    TwoFactorChallengeFailure,
    EnableTwoFactor,
    DisableTwoFactor,
    AddFallbackPhone,
    RemoveFallbackPhone
}