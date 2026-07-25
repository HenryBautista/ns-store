# Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props ./
COPY src/NsStore.Domain/*.csproj src/NsStore.Domain/
COPY src/NsStore.Application/*.csproj src/NsStore.Application/
COPY src/NsStore.Infrastructure/*.csproj src/NsStore.Infrastructure/
COPY src/NsStore.Api/*.csproj src/NsStore.Api/
RUN dotnet restore src/NsStore.Api/NsStore.Api.csproj

COPY src/ src/
RUN dotnet publish src/NsStore.Api/NsStore.Api.csproj -c Release -o /app

# Run
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app ./

# Non-root by default; secrets arrive through environment variables.
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "NsStore.Api.dll"]
