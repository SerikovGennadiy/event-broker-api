FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["EventBroker.Contracts/Enums/Enums.csproj", "EventBroker.Contracts/Enums/"]
COPY ["EventBroker.Contracts/Messaging/Messaging.csproj", "EventBroker.Contracts/Messaging/"]

COPY ["EventBroker.Services/Users/Presentation/Users.API.csproj", "EventBroker.Services/Users/Presentation/"]
COPY ["EventBroker.Services/Users/Application/Users.Application.csproj", "EventBroker.Services/Users/Users.Application/"]
COPY ["EventBroker.Services/Users/Domain/Users.Domain.csproj", "EventBroker.Services/Users/Users.Domain/"]
COPY ["EventBroker.Services/Users/Infrastructure/Users.Infrastructure.csproj", "EventBroker.Services/Users/Users.Infrastructure/"]

RUN dotnet restore "EventBroker.Services/Users/Presentation/Users.API.csproj"

COPY . .
WORKDIR "/src/EventBroker.Services/Users/Presentation"
RUN dotnet publish "Users.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS final
WORKDIR /app

EXPOSE 8080
EXPOSE 8081

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Users.API.dll"]