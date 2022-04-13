# Nuages Identity

### What is Nuages Identity

Nuages Identity is an ASP.NET Core application implementing ASP.NET Identity in a different way.

Try it now!  https://identity.nuages.org (hosted on AWS using ECS .25 vCPU | 2 GB)

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

- Introduce dependies on AWS. 
  - System Manager
    - AppConfig
    - Parameter Store
  - SES
  - SNS
  - Secret Manager
- LigerShark.WebOptimizer.Code https://github.com/ligershark/WebOptimizer
- Font Awesome 6 https://fontawesome.com/
- Vue 3 https://vuejs.org/
- NLog https://nlog-project.org/
- Macross.Json.Extensions https://github.com/Macross-Software/core/tree/develop/ClassLibraries/Macross.Json.Extensions
- HtmlAgilityPack https://html-agility-pack.net/

### Gettings Started

By default, the application will run with the following settings

- InMemory data storage
- Email are sent to the console output 
- No Google Recaptcha
- No Google OpenIdProvider
- Two (2) demo clients created for OpenIdDict

Those settings can be changed using standard configuration mechanism.

### Run locally

``` cd src/Nuages.Identity.UI
cd src/Nuages.Identity.UI
dotnet run
```

Application will be available at https://localhost:8002

### Run with Docker

```
docker build -t nuages.identity.ui .
docker run -it --rm -p 8002:80 --env-file ./env.list --name nuage-identity nuages.identity.ui
```

Application will be available at https://localhost:8002
