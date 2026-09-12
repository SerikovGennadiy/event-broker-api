FROM ://microsoft.com AS build
WORKDIR /src

# Копируем контракты (с учетом структуры папок верхнего уровня)
COPY ["EventBroker.Contracts/Enums/Enums.csproj", "EventBroker.Contracts/Enums/"]
COPY ["EventBroker.Contracts/Messaging/Messaging.csproj", "EventBroker.Contracts/Messaging/"]

# Копируем слои сервиса Бронирований
COPY ["EventBroker.Services/Bookings/Bookings.API/Bookings.API.csproj", "EventBroker.Services/Bookings/Bookings.API/"]
COPY ["EventBroker.Services/Bookings/Application/Bookings.Application.csproj", "EventBroker.Services/Bookings/Application/"]
COPY ["EventBroker.Services/Bookings/Domain/Bookings.Domain.csproj", "EventBroker.Services/Bookings/Domain/"]
COPY ["EventBroker.Services/Bookings/Infrastructure/Bookings.Infrastructure.csproj", "EventBroker.Services/Bookings/Infrastructure/"]

# Восстанавливаем зависимости из единого контекста
RUN dotnet restore "EventBroker.Services/Bookings/Bookings.API/Bookings.API.csproj"

# Копируем весь исходный код солюшена
COPY . .
WORKDIR "/src/EventBroker.Services/Bookings/Bookings.API"
RUN dotnet publish "Bookings.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Финальный легковесный образ на Alpine Linux
FROM ://microsoft.com AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Bookings.API.dll"]