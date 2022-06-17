// ReSharper disable InconsistentNaming
namespace Nuages.Identity.Services;

public enum IdentityEvents
{
    Login,
    FailedLoginUserIsNotConfirmed,
    LockingOutUser,
    FailedLoginUserIsLockedOut,
    Register,
    LoginFailed,
    Login2FASuccess,
    Login2FAFailed,
    LoginRecoveryCodeSuccess,
    LoginRecoveryCodeFailed,
    LoginSMSSuccess,
    LoginSMSFailed,
    LoginSMSCodeSent,
    ForgetPasswordUrlSent,
    LoginSMSCodeNotAvailable,
    ForgetPasswordUrlNotAvailable,
    ResetPasswordFailed,
    ResetPasswordFailedUserNotFound,
    ResetPasswordSuccess,
    MagicLinkSent,
    MagicLinkFailed,
    MagicLinkFailedUserNotFound,
    ConfirmEmailSuccess,
    ConfirmEmailFailed,
    ConfirmationEmailSent,
    ConfirmationEmailFailed,
    EmailChangeFailedSuccess,
    EmailChangeFailedAlreadyExists,
    PasswordChanged,
    PasswordAdded,
    PhoneRemoved,
    PhoneAdded,
    UserNameChanged,
    ProfileChanged,
    EmailChangedEmailSent,
    SMSVerificationCodeSent,
    MFADisabled,
    MFAEnabled,
    MFARecoveryCodeReset,
    Fido2CredentialAdded,
    Fido2CredentialRemoved
}