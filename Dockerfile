


FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build

COPY . .

RUN dotnet restore "src/Nuages.Identity.UI/Nuages.Identity.UI.csproj"

WORKDIR "src/Nuages.Identity.UI/"

RUN apt-get update
RUN apt-get -y install curl gnupg
RUN curl -sL https://deb.nodesource.com/setup_17.x  | bash -
RUN apt-get -y install nodejs
RUN npm install

RUN dotnet build Nuages.Identity.UI.csproj -c Release -o /app/build --framework net7.0

FROM build AS publish
RUN dotnet publish  Nuages.Identity.UI.csproj --configuration Release --framework net7.0 --self-contained false /p:GenerateRuntimeConfigurationFiles=true --runtime linux-x64 -o /app/publish

RUN dotnet dev-certs https -ep /app/aspnetapp.pfx -p TestPassword

FROM base AS final

COPY --from=publish /app/publish .
COPY Readme.md *.pfx ./



ENTRYPOINT ["dotnet", "Nuages.Identity.UI.dll"]
