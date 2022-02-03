using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Amazon;
using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using Microsoft.AspNetCore.Identity;
using Nuages.AspNetIdentity.Mongo;
using Nuages.Identity.Services;
using Nuages.Identity.UI.OpenIdDict;
using Nuages.Localization;
using OpenIddict.Abstractions;

namespace Nuages.Identity.UI;

[ExcludeFromCodeCoverage]
public class Startup
{
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration, IWebHostEnvironment env)
    {
        _configuration = configuration;
        _env = env;
    }

    // This method gets called by the runtime. Use this method to add services to the container
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddDataProtection()
            .PersistKeysToAWSSystemsManager("Nuages.Identity.UI/DataProtection");

        services.AddHttpClient();

        AWSXRayRecorder.InitializeInstance(_configuration);

        if (!_env.IsDevelopment())
        {
            AWSSDKHandler.RegisterXRayForAllServices();
            AWSXRayRecorder.RegisterLogger(LoggingOptions.Console);
        }
        else
        {
            AWSXRayRecorder.RegisterLogger(LoggingOptions.None);
        }

        services.AddNuagesIdentity(_configuration,
            _ => { },
            identity =>
            {
                identity.User = new UserOptions
                {
                    RequireUniqueEmail = true /* Not the default*/
                };

                identity.ClaimsIdentity = new ClaimsIdentityOptions
                {
                    RoleClaimType = OpenIddictConstants.Claims.Role,
                    UserNameClaimType = OpenIddictConstants.Claims.Name,
                    UserIdClaimType = OpenIddictConstants.Claims.Subject
                };

                identity.SignIn = new SignInOptions
                {
                    RequireConfirmedEmail = false,
                    RequireConfirmedPhoneNumber = false, //MUST be false
                    RequireConfirmedAccount = false //MUST be false
                };
            })
        .AddMongoStorage(options =>
        {
            options.ConnectionString = _configuration["Nuages:Mongo:ConnectionString"];
            options.Database = _configuration["Nuages:Mongo:Database"];
        });

        services.AddNuagesAuthentication()
            .AddGoogle(googleOptions =>
            {
                googleOptions.ClientId = _configuration["Google:ClientId"];
                googleOptions.ClientSecret = _configuration["Google:ClientSecret"];
            });

        // ReSharper disable once UnusedParameter.Local
        services.AddNuagesOpenIdDict(_configuration, configure => { });
        
        services
            .AddMvc()
            .AddJsonOptions(jsonOptions =>
            {
                jsonOptions.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            })
            .AddNuagesLocalization(_configuration);

        services.AddHttpContextAccessor();

        services.AddWebOptimizer( !_env.IsDevelopment(),  !_env.IsDevelopment());

        services.AddUI(_configuration);

        services.AddHealthChecks();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            //app.UseDeveloperExceptionPage();
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseWebOptimizer();
        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseRequestLocalization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapRazorPages();
            endpoints.MapControllers();
            endpoints.MapHealthChecks("health");
        });
    }
}