namespace Nuages.Identity.Services.AspNetIdentity;

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