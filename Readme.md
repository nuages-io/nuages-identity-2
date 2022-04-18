# Nuages Identity

### What is Nuages Identity

Nuages Identity is an ASP.NET Core application implementing ASP.NET Identity in a different way.

Try it now!  https://identity.nuages.org (hosted on AWS using ECS .25 vCPU | 1 GB)

### What is different?

- Multi Language support (English and french included) https://github.com/nuages-io/nuages-localization
- UI build with Vue 3 and Tailwind CSS https://tailwindcss.com/  (Dark and light theme)
- Implement OpenIddict (client credential, device, authorization code, password flows) https://github.com/openiddict/openiddict-core
- Implement Fido2 as 2FA method https://github.com/passwordless-lib/fido2-net-lib
- Add SMS 2FA fallback
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

### Dependencies

- AWS decencies ar disabled by default. See next section for additional information.
- LigerShark.WebOptimizer.Code https://github.com/ligershark/WebOptimizer
- Font Awesome 6 https://fontawesome.com/
- Vue 3 https://vuejs.org/
- NLog https://nlog-project.org/
- Macross.Json.Extensions https://github.com/Macross-Software/core/tree/develop/ClassLibraries/Macross.Json.Extensions
- HtmlAgilityPack https://html-agility-pack.net/



### Dependencies when UseAWS flag is true

- System Manager
  - AppConfig
  - Parameter Store
- Simple Email Service (SES)
- Simple Notification Service (SNS)
- Secret Manager
- ElastiCache REDIS

### Gettings Started

By default, the application will run with the following settings

- InMemory data storage
- Email are sent to the console output 
- No Google Recaptcha
- No Google OpenIdProvider
- Two (2) demo clients created for OpenIdDict

Those settings can be changed using standard configuration mechanism.

### Configuration

Configuration is done using the standard ICOnfiguration system. You may want to use one of the following ways to customize the application.

- Change appsettings.json
- Add a appsettings.local.json and/or appesttings.prod.json (those file are not added to git)
- Use environment variables
- If using AWS
  - Use AppConfig
  - Use ParameterStore

##### Data storage options

```json
"Data" :
    {
      "Storage": "InMemory",
      "ConnectionString" : "",
      "Redis" : ""
    }
```



##### Identity options

``` json
"Nuages":
{
  "Identity": {
      "Name": "Nuages",
      "Authority": "https://localhost:8001",
      "SupportsAutoPasswordExpiration": true,
      "AutoExpirePasswordDelayInDays": 60,
      "SupportsLoginWithEmail": true,
      "AutoConfirmExternalLogin": true,
      "EnablePasswordHistory" : "true",
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
```

##### UI options

```json
"Nuages":
{
   "UI": {
      "ShowRegistration": true,
      "ExternalLoginAutoEnrollIfEmailExists": true,
      "ExternalLoginPersistent": true,
      "EnableMagicLink": true,
      "EnablePhoneFallback": true,
      "Enable2FARememberDevice": true,
      "EnableFido2": true,
      "FontAwesomeUrl": "https://kit.fontawesome.com/70b74b4315.js"
    }
}
```

##### Localization options

```json
"Nuages":
{
  "Localization": {
      "DefaultCulture": "fr-CA",
      "LangClaim": "lang",
      "Cultures": [
        "fr-CA",
        "en-CA"
      ]
  }
}
```

See https://github.com/nuages-io/nuages-localization for more localization information



##### OpenIdDict options

```json
"Nuages": 
{
	"OpenIdDict": {
      "EncryptionKey": "",
      "SigningKey": "",
      "CreateDemoClients" : true
  }
}
```



##### Google Racaptcha

```json
"Nuages" : 
{
	"Web": {
      "GoogleRecaptcha": {
        "SiteKey": "",
        "SecretKey": ""
      }
    }
}
```

##### OAuth provider

```json
"Nuages" : 
{
	"OpenIdProviders": {
      "Google": {
        "ClientId": "",
        "ClientSecret": ""
      }
    }
}
```



### Configuration with AWS

##### System Manager options

```json
"Nuages" : 
{
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
```





##### Using SecretManager

You can use a secret instead of a string value for any configuration value.

Ex. Let's says you want to hide the database connection string

So instead of

``` json
"Data" :
    {
      "Storage": "MongoDb",
      "ConnectionString" : "my connection string value"
    }
```

You can swap the value for a secret ARN (the ARN can be found in your AWS account)

```json
"Data" :
{
  "Storage": "MongoDb",
  "ConnectionString" : " arn:aws:secretsmanager:{region}:{accounbt_id}:secret:identity/mongo-ABC123"
}
```

Only string values are supported.



### Running the application

### Run locally

``` sh
cd src/Nuages.Identity.UI
dotnet run
```

Application will be available at https://localhost:8002

### Run locally with Docker

```shell
docker build -t nuages.identity.ui .
docker run -it --rm -p 8003:80 -e Nuages__Identity__Authority=http://localhost:8003 --name nuage-identity nuages.identity.ui
```

Application will be available at http://localhost:8003 (no HTTPS)

### Coming next !

- Management API
- Management UI
