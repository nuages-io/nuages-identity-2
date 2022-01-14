using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json.Serialization;
using Amazon;
using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Handlers.AwsSdk;

using Microsoft.IdentityModel.Tokens;
using Nuages.Localization;
using Nuages.Web;

namespace Nuages.Identity.API;

[ExcludeFromCodeCoverage]
public class Startup
{
    private readonly IWebHostEnvironment _env;

    public Startup(IConfiguration configuration, IWebHostEnvironment env)
    {
        Configuration = configuration;
        _env = env;
    }

    private IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container
    public void ConfigureServices(IServiceCollection services)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        
        
        services.AddHttpClient();
        services.AddEndpointsApiExplorer();
        
        services.AddSingleton(Configuration);
            
        services.AddHttpContextAccessor();
            
        if (!_env.IsDevelopment())
        {
            AWSXRayRecorder.InitializeInstance(Configuration);
            AWSXRayRecorder.RegisterLogger(LoggingOptions.Console);
            AWSSDKHandler.RegisterXRayForAllServices();
        }
        
        services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        }).AddNuagesLocalization(Configuration);


        services.AddAuthentication().AddJwtBearer("Bearer", options =>
        {
            options.Authority = "https://localhost:8003/";

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ActorValidationParameters = null,
                AlgorithmValidator = null,
                AudienceValidator = null,
                AuthenticationType = null,
                ClockSkew = default,
                CryptoProviderFactory = null,
                IgnoreTrailingSlashWhenValidatingAudience = false,
                IssuerSigningKeyValidator = null,
                IssuerSigningKey = null,
                IssuerSigningKeyResolver = null,
                IssuerSigningKeys = null,
                IssuerValidator = null,
                LifetimeValidator = null,
                NameClaimType = null,
                NameClaimTypeRetriever = null,
                PropertyBag = null,
                RequireAudience = false,
                RequireExpirationTime = false,
                RequireSignedTokens = false,
                RoleClaimType = null,
                RoleClaimTypeRetriever = null,
                SaveSigninToken = false,
                SignatureValidator = null,
                TokenDecryptionKey = null,
                TokenDecryptionKeyResolver = null,
                TokenDecryptionKeys = null,
                TokenReader = null,
                TokenReplayCache = null,
                TokenReplayValidator = null,
                TryAllIssuerSigningKeys = false,
                TypeValidator = null,
                ValidateActor = false,
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = false,
                ValidateLifetime = false,
                ValidateTokenReplay = false,
                ValidAlgorithms = null,
                ValidAudience = null,
                ValidAudiences = null,
                ValidIssuer = null,
                ValidIssuers = null,
                ValidTypes = null

            };
        });

        // services.AddAuthorization(o => o.AddPolicy("Authenticated", 
        //     b => b.RequireAuthenticatedUser()));

        
        services.AddDataProtection()
            .PersistKeysToAWSSystemsManager($"{Configuration["Nuages:Sender:StackName"]}/DataProtection");
        
        services.AddHealthChecks();
        
        services.AddSwaggerDocument(config =>
        {
            config.PostProcess = document =>
            {
                document.Info.Version = "v1";
                document.Info.Title = "Nuages Sender Service";

                document.Info.Contact = new NSwag.OpenApiContact
                {
                    Name = "Nuages.io",
                    Email = string.Empty,
                    Url = "https://github.com/nuages-io/nuages-sender"
                };
                document.Info.License = new NSwag.OpenApiLicense
                {
                    Name = "Use under LICENCE",
                    Url = "http://www.apache.org/licenses/LICENSE-2.0"
                };
            };
        });
        
        // services.AddHttpLogging(logging =>
        // {
        //     logging.LoggingFields = HttpLoggingFields.ResponseBody;
        //     logging.ResponseBodyLogLimit = 4096;
        // });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        //app.UseHttpLogging();
        
        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapCustomEndpoints(serviceProvider);
            endpoints.MapHealthChecks("/health");
        });
        
        app.UseOpenApi();
        app.UseSwaggerUi3();

    }
}