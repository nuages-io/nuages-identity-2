#dotnet test ./Nuages.Sender.Services.Tests/Nuages.Sender.Services.Tests.csproj
#dotnet test ./Nuages.Sender.Services.Tests.Integration/Nuages.Sender.Services.Tests.Integration.csproj

dotnet publish ./Nuages.Identity.API/Nuages.Identity.API.csproj --configuration Release --framework net6.0 --self-contained false /p:GenerateRuntimeConfigurationFiles=true --runtime linux-x64
dotnet publish ./Nuages.Identity.UI/Nuages.Identity.UI.csproj --configuration Release --framework net6.0 --self-contained false /p:GenerateRuntimeConfigurationFiles=true --runtime linux-x64