FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["SpbuVkApiCrawler/SpbuVkApiCrawler.csproj", "SpbuVkApiCrawler/"]
RUN dotnet restore "SpbuVkApiCrawler/SpbuVkApiCrawler.csproj"
COPY . .
WORKDIR "/src/SpbuVkApiCrawler"
RUN dotnet build "SpbuVkApiCrawler.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SpbuVkApiCrawler.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SpbuVkApiCrawler.dll"]
