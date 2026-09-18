FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# 1. Копируем общие контракты солюшена
COPY ["EventBroker.Contracts/Enums/Enums.csproj", "EventBroker.Contracts/Enums/"]
COPY ["EventBroker.Contracts/Messaging/Messaging.csproj", "EventBroker.Contracts/Messaging/"]

# 2. Копируем слои микросервиса Бронирований
COPY ["EventBroker.Services/Bookings/Presentation/Bookings.API.csproj", "EventBroker.Services/Bookings/Presentation/"]
COPY ["EventBroker.Services/Bookings/Application/Bookings.Application.csproj", "EventBroker.Services/Bookings/Application/"]
COPY ["EventBroker.Services/Bookings/Domain/Bookings.Domain.csproj", "EventBroker.Services/Bookings/Domain/"]
COPY ["EventBroker.Services/Bookings/Infrastructure/Bookings.Infrastructure.csproj", "EventBroker.Services/Bookings/Infrastructure/"]

# 3. Восстанавливаем зависимости
RUN dotnet restore "EventBroker.Services/Bookings/Presentation/Bookings.API.csproj"

# 4. Копируем исходный код и публикуем Release-билд
COPY . .
WORKDIR "/src/EventBroker.Services/Bookings/Presentation"
RUN dotnet publish "Bookings.API.csproj" -c Release -o /app/publish /p:UseAppHost=false


FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS final
WORKDIR /app

EXPOSE 8080
EXPOSE 8081

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Bookings.API.dll"]