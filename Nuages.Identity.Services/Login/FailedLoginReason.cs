namespace Nuages.Identity.Services.Login;

public enum FailedLoginReason
{
    NotWithinDateRange,
    PasswordMustBeChanged,
    EmailNotConfirmed,
    PhoneNotConfirmed,
    AccountNotConfirmed,
    PasswordExpired,
    RecaptchaError,
    LockedOut,
    UserNameOrPasswordInvalid,
    FailedRecoveryCode,
    FailedMfa,
    FailedSMS
}