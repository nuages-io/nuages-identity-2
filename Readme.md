# Nuages Identity

### What is Nuages Identity

Nuages Identity is an ASP.NET Core application implementing ASP.NET Identity in a dirrent way than the samples templates provided by Microsoft.

Try it now!  https://identity.nuages.org

### What is different?

- Multi Language support (English and french included) https://github.com/nuages-io/nuages-localization
- UI build with Tailwind CSS https://tailwindcss.com/ and VueJs (Dark and light theme)
- Implement OpenIddict OAuth (client credential, device, authorization code, password) https://github.com/openiddict/openiddict-core
- Implement Fido2 as 2FA method https://github.com/passwordless-lib/fido2-net-lib
- Add SMS 2FA fallback
- Passwordless login using Magic Link
- Message service for sending Email ans SMS (using AWS SES). Basic email templates provided in English and French.
- Support Google ReCaptcha
- Support password reuse restriction
- Support password expiration
- Support user must change password flag
- And more...


### Database storage

Support is provided for the following Database engine. More can be added.

- MongoDD
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
- VueJs
- NLog
- Macross.Json.Extensions
- HtmlAgilityPack

### Run with Docker

- docker build -t nuages.identity.ui .
- docker run -it --rm -p 8002:80 --env-file ./env.list --name nuage-identity nuages.identity.ui
