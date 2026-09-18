# Этап 1
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# 1. Копируем .csproj в ту же относительную структуру, что и в репозитории
COPY ["EventBroker.API/EventBroker.API.csproj", "EventBroker.API/"]

# 2. Восстанавливаем зависимости
RUN dotnet restore "EventBroker.API/EventBroker.API.csproj"

# 3. Копируем всё остальное решение
COPY . .

# 4. Публикуем
WORKDIR "/src/EventBroker.API"
RUN dotnet publish "EventBroker.API.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# Этап 2
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "EventBroker.API.dll"]