namespace Nuages.Identity.Services;

public enum FailedLoginReason
{
    NotWithinDateRange,
    PasswordMustBeChanged,
    EmailNotConfirmed,
    PhoneNotConfirmed,
    AccountNotConfirmed,
    PasswordExpired,
    PasswordNeverSet,
    RecaptchaError
}