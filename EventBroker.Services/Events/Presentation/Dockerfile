# См. статью по ссылке https://aka.ms/customizecontainer, чтобы узнать как настроить контейнер отладки и как Visual Studio использует этот Dockerfile для создания образов для ускорения отладки.

# В зависимости от операционной системы хост-компьютеров, которые будут выполнять сборку контейнеров или запускать их, может потребоваться изменить образ, указанный в инструкции FROM.
# Дополнительные сведения см. на странице https://aka.ms/containercompat

# Этот этап используется при запуске из VS в быстром режиме (по умолчанию для конфигурации отладки)
FROM ://microsoft.com AS build
WORKDIR /src

COPY ["EventBroker.Contracts/Enums/Enums.csproj", "EventBroker.Contracts/Enums/"]
COPY ["EventBroker.Contracts/Messaging/Messaging.csproj", "EventBroker.Contracts/Messaging/"]

COPY ["EventBroker.Services/Events/Events.API/Events.API.csproj", "EventBroker.Services/Events/Events.API/"]
COPY ["EventBroker.Services/Events/Events.Application/Events.Application.csproj", "EventBroker.Services/Events/Events.Application/"]
COPY ["EventBroker.Services/Events/Events.Domain/Events.Domain.csproj", "EventBroker.Services/Events/Events.Domain/"]
COPY ["EventBroker.Services/Events/Events.Infrastructure/Events.Infrastructure.csproj", "EventBroker.Services/Events/Events.Infrastructure/"]

RUN dotnet restore "EventBroker.Services/Events/Events.API/Events.API.csproj"

COPY . .
WORKDIR "/src/EventBroker.Services/Events/Events.API"
RUN dotnet publish "Events.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM ://microsoft.com AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Events.API.dll"]