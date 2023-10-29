FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build

# Install Node.js
RUN curl -fsSL https://deb.nodesource.com/setup_14.x | bash - \
    && apt-get install -y \
        nodejs \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /src
COPY ["CityWeather/CityWeather.csproj", "CityWeather/"]
RUN dotnet restore "CityWeather/CityWeather.csproj"
COPY . .
WORKDIR "/src/CityWeather"
RUN dotnet build "CityWeather.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CityWeather.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CityWeather.dll"]
