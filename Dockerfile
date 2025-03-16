FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env
WORKDIR /app

ENV DBCONNECTION="Host=localhost;Port=5432;Database=extensionMarket;Username=postgres;Password=123456"
ENV JWT_KEY=',$4Eb]Nee,YKRYs2~t.w(?9Yep{IZ=a!'

COPY . ./
WORKDIR /app
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build-env /app/out .
COPY ./Migrations .

ENV ASPNETCORE_URLS="http://0.0.0.0:80"

EXPOSE 80

ENTRYPOINT [ "dotnet", "ExtensionMarket.dll" ]