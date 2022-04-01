// ReSharper disable InconsistentNaming


using Nuages.Web.Recaptcha;

namespace Nuages.Identity.UI;

public class UIOptions
{
    public bool ShowRegistration { get; set; }

    public bool ExternalLoginAutoEnrollIfEmailExists { get; set; }
    public bool ExternalLoginPersistent { get; set; }
    public bool Bypass2FAWithExternalLogin { get; set; } = true;

    public bool EnablePasswordless { get; set; } = true;
    public bool EnablePhoneFallback { get; set; } = true;
    public bool Enable2FARememberDevice { get; set; } = true;
    public bool EnableFido2 { get; set; } = true;
    public string FontAwesomeUrl { get; set; } = string.Empty;
}

public static class UIConfigExtension
{
    public static void AddUI(this IServiceCollection services, IConfiguration configuration, Action<UIOptions>? setupAction = null)
    {
        services.Configure<UIOptions>(configuration.GetSection("Nuages:UI"));

        if (setupAction != null)
        {
            services.Configure(setupAction);
        }

        services.AddGoogleRecaptcha(configuration);
    }
}