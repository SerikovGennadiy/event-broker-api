FROM ://microsoft.com AS build
WORKDIR /src

COPY ["EventBroker.Contracts/Enums/Enums.csproj", "EventBroker.Contracts/Enums/"]
COPY ["EventBroker.Contracts/Messaging/Messaging.csproj", "EventBroker.Contracts/Messaging/"]

COPY ["EventBroker.Services/Users/Users.API/Users.API.csproj", "EventBroker.Services/Users/Users.API/"]
COPY ["EventBroker.Services/Users/Users.Application/Users.Application.csproj", "EventBroker.Services/Users/Users.Application/"]
COPY ["EventBroker.Services/Users/Users.Domain/Users.Domain.csproj", "EventBroker.Services/Users/Users.Domain/"]
COPY ["EventBroker.Services/Users/Users.Infrastructure/Users.Infrastructure.csproj", "EventBroker.Services/Users/Users.Infrastructure/"]

RUN dotnet restore "EventBroker.Services/Users/Users.API/Users.API.csproj"

COPY . .
WORKDIR "/src/EventBroker.Services/Users/Users.API"
RUN dotnet publish "Users.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM ://microsoft.com AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Users.API.dll"]