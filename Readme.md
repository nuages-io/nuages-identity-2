# Nuages Identity



### What is Nuages Identity

Nuages Identity is an ASP.NET Core application implementing ASP.NET Identity. The main goal is to provide a production-ready solution, not just startup sample  project.

Try it now!  https://identity.nuages.org 



### What is included?

- Ready to use as-is
- Multi Language support (English and french included) https://github.com/nuages-io/nuages-localization
- Implement OpenIddict (client credential, device, authorization code, password flows) https://github.com/openiddict/openiddict-core
- Implement Fido2 as 2FA method https://github.com/passwordless-lib/fido2-net-lib
- UI build with Vue 3 and Tailwind CSS https://tailwindcss.com/  (Dark and light theme)
- SMS 2FA fallback
- Login using Magic Link
- Message service for sending Email ans SMS (using AWS SES). Basic email templates provided in English and French.
- Support Google ReCaptcha
- Support password reuse restriction
- Support password expiration
- Support user must change password flag
- And more...



### Database storage

Support is provided for the following Database engine. 

- InMemory (default)
- MongoDB
- SqlServer
- MySql



## Gettings Started

By default, the application will run with the following settings

- InMemory data storage
- Email are sent to the console output 
- No Google Recaptcha
- No OAuth providers
- Two (2) demo clients created for OpenIdDict

Those settings can be changed using standard configuration mechanism.

### Run locally

From root directory,

``` sh
cd src/Nuages.Identity.UI
dotnet run
```

Application will be available at https://localhost:8001

### Run locally with Docker

From the root directory,

```shell
docker build -t nuages.identity.ui .
docker run -it --rm -p 8003:80 --env-file ./env.list --name nuage-identity nuages.identity.ui
```

Application will be available at http://localhost:8003 (no HTTPS)

Note: env.list must include environment variables required to run the app (see Configuratio below)

## Configuration

Configuration is done using the standard Configuration system. You may want to use one of the following ways to customize the application.

- Change appsettings.json
- Add a appsettings.local.json and/or appesettings.prod.json (those file are not added to git)
- Use environment variables
- If using AWS
  - You may use AppConfig
  - You may use ParameterStore

#### Data storage options

```json
{
  "Nuages": {
    "Data": {
      "Storage": "InMemory",
      "ConnectionString": "",
      "Redis": ""
    }
  }
}
```

- **Nuages\__Data__Storage** : InMemory, MongoDb, SqlServer or MySql.
- **Nuages\__Data__ConnectionString**: Your database connection string
- **Nuages\__Data__Redis**: Optional Redis connection string. If provided, it will be used as the distributed cache mechanism (IDistributedCache).



#### Identity options

``` json
{
  "Nuages": {
    "Identity": {
      "Name": "Nuages",
      "Authority": "https://localhost:8001",
      "SupportsAutoPasswordExpiration": true,
      "AutoExpirePasswordDelayInDays": 60,
      "SupportsLoginWithEmail": true,
      "AutoConfirmExternalLogin": true,
      "EnablePasswordHistory": "true",
      "PasswordHistoryCount": 5,
      "Audiences": [
        "IdentityAPI"
      ],
      "Password": {
        "RequiredLength": 6,
        "RequireNonAlphanumeric": true,
        "RequireLowercase": true,
        "RequireUppercase": true,
        "RequireDigit": true,
        "RequiredUniqueChars": 1
      }
    }
  }
}
```

- **Nuages\__Identity__Name** : Name of your application
- **Nuages\__Identity__Authority** : Your authority URL. It must match your public URL.
- **Nuages\__Identity__SupportsAutoPasswordExpiration** : Set to true will auto expire password after AutoExpirePasswordDelayInDays days.
- **Nuages\__Identity__AutoExpirePasswordDelayInDays** : If Enabled, the number of days before a password expires.
- **Nuages\__Identity__SupportsLoginWithEmail** : Set to true to allow loging in by email AND user name.
- **Nuages\__Identity__AutoConfirmExternalLogin** : Set to true to auto verified user registerd using an external OAuth provider
- **Nuages\__Identity__EnablePasswordHistory** : Set to true to prevent reusing password
- **Nuages\__Identity__PasswordHistoryCount** : Number of password to remember
- **Nuages\__Identity__Audiences** : The Audiences supported by the authority.
- **Nuages\__Identity__Password**: Password options



#### UI Options

```json
{
  "Nuages": {
    "UI": {
      "AllowSelfRegistration": true,
      "ExternalLoginAutoEnrollIfEmailExists": true,
      "ExternalLoginPersistent": true,
      "EnableMagicLink": true,
      "EnablePhoneFallback": true,
      "Enable2FARememberDevice": true,
      "EnableFido2": true,
      "FontAwesomeUrl": "https://kit.fontawesome.com/70b74b4315.js"
    }
  }
}
```

- **Nuages\__UI__AllowSelfRegistration** : Set to true to let user create their own account.
- **Nuages\__UI__ExternalLoginAutoEnrollIfEmailExists** : Set to true to automatically bind external login to user account when the email match.
- **Nuages\__UI__ExternalLoginPersistent** : Set tot true to remember external login
- **Nuages\__UI__EnableMagicLink** : Set to true to allow magic link login
- **Nuages\__UI__EnablePhoneFallback** : Set to true to allow SMS as an alterntive 2FA mechanism
- **Nuages\__UI__Enable2FARememberDevice** : Set to true to allow remembering 2FA login on the device
- **Nuages\__UI__EnableFido2** : Set to true to allow Fido2 as a 2FA alternative
- **Nuages\__UI__FontAwesomeUrl** : Your Font Awesome 6 Kit URL



#### Localization options

```json
{
  "Nuages": {
    "Localization": {
      "DefaultCulture": "fr-CA",
      "LangClaim": "lang",
      "Cultures": [
        "fr-CA",
        "en-CA"
      ]
    }
  }
}
```

See https://github.com/nuages-io/nuages-localization for more localization information



#### OpenIdDict options

```json
{
  "Nuages": {
    "OpenIdDict": {
      "EncryptionKey": "",
      "SigningKey": "",
      "CreateDemoClients": true
    }
  }
}
```



#### Google Racaptcha

```json
{
  "Nuages": {
    "Web": {
      "GoogleRecaptcha": {
        "SiteKey": "",
        "SecretKey": ""
      }
    }
  }
}
```



#### OAuth Provider

```json
{
  "Nuages": {
    "OpenIdProviders": {
      "Google": {
        "ClientId": "",
        "ClientSecret": ""
      },
      "Microsoft": {
        "ClientId": "",
        "ClientSecret": ""
      },
      "Facebook": {
        "AppId": "",
        "AppSecret": ""
      },
      "GitHub": {
        "ClientId": "",
        "ClientSecret": ""
      }
    }
  }
}
```



## Configuration with AWS



#### AWS System Manager options

```json
{
  "Nuages": {
    "ApplicationConfig": {
      "ParameterStore": {
        "Enabled": false,
        "Path": "/NuagesIdentity"
      },
      "AppConfig": {
        "Enabled": false,
        "ApplicationId": "NuagesIdentity",
        "EnvironmentId": "Prod",
        "ConfigProfileId": "WebUI"
      }
    }
  }
}
```



Application settings can be set using System Manager **ParameterStore** and **AppConfig**. 

Set Enable to true to activate.

More info here https://github.com/aws/aws-dotnet-extensions-configuration



#### Using AWS SecretManager

You can use a secret instead of a string value for any configuration value.

Ex. Let's says you want to hide the database connection string

So instead of

``` json
{
  "Nuages": {
    "Data": {
      "ConnectionString": "my connection string value"
    }
  }
}
```

You can swap the value for a secret ARN (the ARN can be found in your AWS account)

```json
{
  "Nuages": {
    "Data": {
      "ConnectionString": "arn:aws:secretsmanager:{region}:{account_id}:secret:identity/mongo-ABC123"
    }
  }
}
```

Only string values are supported.



---

### Dependencies

- AWS services are disabled by default. See next section for additional information.
- LigerShark.WebOptimizer.Code https://github.com/ligershark/WebOptimizer
- Font Awesome 6 https://fontawesome.com/
- Vue 3 https://vuejs.org/
- NLog https://nlog-project.org/
- Macross.Json.Extensions https://github.com/Macross-Software/core/tree/develop/ClassLibraries/Macross.Json.Extensions
- OctoKit (to get GitHub user email) (https://github.com/octokit/octokit.net)  (Optional)
- Microsoft.Extensions.Caching.StackExchangeRedis (Optional)
- Pre-configured OAuth providers (you will have to register your application with the provider)
  - AspNet.Security.OAuth.GitHub
  - Microsoft.AspNetCore.Authentication.Google
  - Microsoft.AspNetCore.Authentication.Facebook
  - Microsoft.AspNetCore.Authentication.Twitter
  - Microsoft.AspNetCore.Authentication.MicrosoftAccount



### Dependencies when UseAWS flag is true

- System Manager
  - AppConfig
  - Parameter Store
- Simple Email Service (SES)
- Simple Notification Service (SNS)
- Secret Manager
- HtmlAgilityPack https://html-agility-pack.net/ (Optional, required by email template loader)

