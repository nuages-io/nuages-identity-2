namespace Nuages.Identity.Services.Login;

public enum MfaMethod
{
    Authenticator,
    SMS,
    RecoveryCode,
    SecurityKeys,
    Mobile
}