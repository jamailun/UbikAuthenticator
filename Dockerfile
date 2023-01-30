FROM mcr.microsoft.com/dotnet/sdk:6.0 AS sdk
WORKDIR /App

COPY . ./

RUN ls -al

RUN dotnet restore
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /App
COPY --from=sdk /App/out .

EXPOSE 80
EXPOSE 443
ENTRYPOINT ["dotnet", "UbikAuthenticator.dll", "--launch-profile Docker"]
