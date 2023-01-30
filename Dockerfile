# Build
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS sdk
WORKDIR /App
COPY . ./
RUN dotnet restore
RUN dotnet publish -c Release -o out

# Add elements
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /App
COPY --from=sdk /App/out .
# Add properties
COPY env.properties /App/env.properties

# Expose & entry points
EXPOSE 80
EXPOSE 443
ENTRYPOINT ["dotnet", "UbikAuthenticator.dll", "--launch-profile Docker"]
