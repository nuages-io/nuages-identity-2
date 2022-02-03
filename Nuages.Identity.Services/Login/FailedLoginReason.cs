namespace Nuages.Identity.Services.Login;

public enum FailedLoginReason
{
    None,
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
    FailedSms
}