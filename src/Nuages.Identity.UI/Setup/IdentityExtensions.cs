using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Nuages.AspNetIdentity.Stores.Mongo;
using Nuages.Fido2.Storage.Mongo;
using Nuages.Identity.Services;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Identity.Services.Email.Sender;
using Nuages.Identity.Services.Email.Sender.AWS;
using Nuages.Identity.Services.Fido2.AspNetIdentity;
using Nuages.Identity.Services.Fido2.Storage;
using Nuages.Identity.UI.OpenIdDict;
using OpenIddict.Abstractions;

namespace Nuages.Identity.UI.Setup;

public static class IdentityExtensions
{
    public static void AddNuagesIdentity(this  IServiceCollection services, IConfiguration configuration)
    {
        var identityBuilder =
            services.AddNuagesAspNetIdentity<NuagesApplicationUser<string>, NuagesApplicationRole<string>, string>(
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
                        RequireConfirmedEmail = true,
                        RequireConfirmedPhoneNumber = false, //MUST be false
                        RequireConfirmedAccount = false //MUST be false
                    };

                    configuration.GetSection("Nuages:Identity:Password").Bind(identity.Password);
                });

        var storage = Enum.Parse<StorageType>(configuration["Nuages:Data:Storage"]);
        var connectionString = configuration["Nuages:Data:ConnectionString"];

        switch (storage)
        {
            case StorageType.SqlServer:
            {
                services.AddDbContext<NuagesIdentityDbContext>(options =>
                {
                    options
                        .UseSqlServer(connectionString);

                    options.UseOpenIddict();
                });

                identityBuilder.AddEntityFrameworkStores<NuagesIdentityDbContext>();

                break;
            }
            case StorageType.MySql:
            {
                services.AddDbContext<NuagesIdentityDbContext>(options =>
                {
                    options
                        .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

                    options.UseOpenIddict();
                });
                identityBuilder.AddEntityFrameworkStores<NuagesIdentityDbContext>();


                break;
            }
            case StorageType.InMemory:
            {
                services
                    .AddDbContext<NuagesIdentityDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("Identity");

                        options.UseOpenIddict();
                    });

                identityBuilder.AddEntityFrameworkStores<NuagesIdentityDbContext>();

                break;
            }
            case StorageType.MongoDb:
            {
                identityBuilder.AddMongoStores<NuagesApplicationUser<string>, NuagesApplicationRole<string>, string>(
                    options =>
                    {
                        options.ConnectionString = connectionString;

                        if (!BsonClassMap.IsClassMapRegistered(typeof(NuagesApplicationUser<string>)))
                            BsonClassMap.RegisterClassMap<NuagesApplicationUser<string>>(cm =>
                            {
                                cm.AutoMap();
                                cm.SetIgnoreExtraElements(true);
                                cm.MapMember(c => c.LastFailedLoginReason)
                                    .SetSerializer(new EnumSerializer<FailedLoginReason>(BsonType.String));
                            });
                    });
                break;
            }
            default:
                throw new Exception("Invalid storage");
        }

        identityBuilder.AddNuagesIdentityServices(configuration, _ => { });

        var uri = new Uri(configuration["Nuages:Identity:Authority"]);

        var fidoBuilder2 = identityBuilder.AddNuagesFido2(options =>
        {
            options.ServerDomain = uri.Host;
            options.ServerName = configuration["Nuages:Identity:Name"];
            options.Origins = new HashSet<string> { configuration["Nuages:Identity:Authority"] };
            options.TimestampDriftTolerance = 300000;
        });

        identityBuilder.AddMessageService(configure =>
        {
            configure.SendFromEmail = configuration["Nuages:MessageService:SendFromEmail"];
            configure.DefaultCulture = configuration["Nuages:MessageService:DefaultCulture"];
        });

        services.AddAWSSender("templates.json");

        switch (storage)
        {
            case StorageType.InMemory:
            {
                services.AddScoped<IFido2Storage, Fido2StorageEntityFramework<NuagesIdentityDbContext>>();
                break;
            }
            case StorageType.MongoDb:
            {
                fidoBuilder2.AddFido2MongoStorage(config =>
                {
                    config.ConnectionString = configuration["Nuages:Data:ConnectionString"];
                });
                break;
            }
            case StorageType.MySql:
            {
                services.AddScoped<IFido2Storage, Fido2StorageEntityFramework<NuagesIdentityDbContext>>();
                break;
            }
            case StorageType.SqlServer:
            {
                services.AddScoped<IFido2Storage, Fido2StorageEntityFramework<NuagesIdentityDbContext>>();
                break;
            }
            default:
                throw new Exception("Invalid storage");
        }

        services.AddNuagesAuthentication()
            .AddGoogle(googleOptions =>
            {
                googleOptions.ClientId = configuration["Nuages:OpenIdProviders:Google:ClientId"];
                googleOptions.ClientSecret = configuration["Nuages:OpenIdProviders:Google:ClientSecret"];
            });
        
        services.AddUI(configuration);
        services.AddNuagesOpenIdDict(configuration, _ => { });
    }
}