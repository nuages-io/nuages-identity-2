// ReSharper disable InconsistentNaming


using Nuages.Web.Recaptcha;

namespace Nuages.Identity.UI;

public class UIOptions
{
    public bool ShowRegistration { get; set; }
    public string RegistrationUrl { get; set; } = "/Account/Register";
    public string LogoUrl { get; set; } = string.Empty;

    public bool ExternalLoginAutoEnrollIfEmailExists { get; set; }
    public bool ExternalLoginPersistent { get; set; }
    public bool Bypass2FAWithExternalLogin { get; set; } = true;

    public bool EnablePasswordless { get; set; } = true;
    public bool EnablePhoneFallback { get; set; } = true;
    public bool Enable2FARememberDevice { get; set; } = true;
}

public static class UIConfigExtension
{
    public static void AddUI(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<UIOptions>(configuration.GetSection("Nuages:UI"));

        services.AddGoogleRecaptcha(configuration);
    }
}