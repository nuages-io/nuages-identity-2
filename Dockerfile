


FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 8001

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore "src/Nuages.Identity.UI/Nuages.Identity.UI.csproj"

WORKDIR "src/Nuages.Identity.UI/"

RUN apt-get update
RUN apt-get -y install curl gnupg
RUN curl -sL https://deb.nodesource.com/setup_17.x  | bash -
RUN apt-get -y install nodejs
RUN npm install

RUN dotnet build "Nuages.Identity.UI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish  Nuages.Identity.UI.csproj --configuration Release --framework net6.0 --self-contained false /p:GenerateRuntimeConfigurationFiles=true --runtime linux-x64 -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Nuages.Identity.UI.dll"]
