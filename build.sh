

#dotnet publish ./Nuages.Identity.API/Nuages.Identity.API.csproj --configuration Release --framework net6.0 --self-contained false /p:GenerateRuntimeConfigurationFiles=true --runtime linux-x64

npm install --prefix ./Nuages.Identity.UI
dotnet publish ./Nuages.Identity.UI/Nuages.Identity.UI.csproj --configuration Release --framework net6.0 --self-contained false /p:GenerateRuntimeConfigurationFiles=true --runtime linux-x64