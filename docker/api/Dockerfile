FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY apps/api/BrowserAgent.Api.csproj ./
RUN dotnet restore BrowserAgent.Api.csproj

COPY apps/api/ ./
RUN dotnet publish BrowserAgent.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/playwright/dotnet:v1.52.0-noble AS runtime

RUN curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 10.0 --runtime aspnetcore --install-dir /usr/share/dotnet

WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development
EXPOSE 8080
ENTRYPOINT ["dotnet", "BrowserAgent.Api.dll"]
