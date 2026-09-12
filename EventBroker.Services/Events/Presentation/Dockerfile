FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["EventBroker.Contracts/Enums/Enums.csproj", "EventBroker.Contracts/Enums/"]
COPY ["EventBroker.Contracts/Messaging/Messaging.csproj", "EventBroker.Contracts/Messaging/"]

COPY ["EventBroker.Services/Events/Presentation/Events.API.csproj", "EventBroker.Services/Events/Presentation/"]
COPY ["EventBroker.Services/Events/Application/Events.Application.csproj", "EventBroker.Services/Events/Events.Application/"]
COPY ["EventBroker.Services/Events/Domain/Events.Domain.csproj", "EventBroker.Services/Events/Events.Domain/"]
COPY ["EventBroker.Services/Events/Infrastructure/Events.Infrastructure.csproj", "EventBroker.Services/Events/Events.Infrastructure/"]

RUN dotnet restore "EventBroker.Services/Events/Presentation/Events.API.csproj"

COPY . .
WORKDIR "/src/EventBroker.Services/Events/Presentation"
RUN dotnet publish "Events.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS final
WORKDIR /app

EXPOSE 8080
EXPOSE 8081

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Events.API.dll"]