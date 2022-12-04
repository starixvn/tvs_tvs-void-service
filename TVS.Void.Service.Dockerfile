FROM hub.nexdev.net/base/dotnet/core/sdk:7.0-alpine AS builder
WORKDIR /src

COPY ./Sources/TicketVoidingSystem ./Sources/TicketVoidingSystem
COPY ./Sources/TVS.Void.Service ./Sources/TVS.Void.Service
COPY ./TVS.Void.Service.Website/TVS.Void.Service.Website.csproj ./TVS.Void.Service.Website/TVS.Void.Service.Website.csproj

WORKDIR /src/TVS.Void.Service.Website
RUN dotnet nuget add source http://nuget.nexdev.net/api/v2
RUN dotnet restore -v n

COPY ./TVS.Void.Service.Website .

RUN dotnet publish -r linux-x64 --self-contained false -o /app

FROM hub.nexdev.net/base/dotnet/core/aspnet:7.0 AS base

# Don’t run as root
RUN adduser --disabled-password \
  --home /app \
  --gecos '' dotnetuser && chown -R dotnetuser /app
USER dotnetuser
#

WORKDIR /app
COPY --from=builder /app .
ENV TZ="America/La_Paz"

ENTRYPOINT ["dotnet", "TVS.Void.Service.Website.dll"]