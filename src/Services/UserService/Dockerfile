# 1. Этап сборки (Build Stage)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Копируем проект SharedContracts и проекты UserService
COPY ["src/Shared/EventsRestApi.Services.SharedContracts/EventsRestApi.Services.SharedContracts.csproj", "src/Shared/EventsRestApi.Services.SharedContracts/"]

COPY ["src/Services/UserService/EventsRestApi.Services.UserService.Domain/EventsRestApi.Services.UserService.Domain.csproj", "src/Services/UserService/EventsRestApi.Services.UserService.Domain/"]
COPY ["src/Services/UserService/EventsRestApi.Services.UserService.Application/EventsRestApi.Services.UserService.Application.csproj", "src/Services/UserService/EventsRestApi.Services.UserService.Application/"]
COPY ["src/Services/UserService/EventsRestApi.Services.UserService.Infrastructure/EventsRestApi.Services.UserService.Infrastructure.csproj", "src/Services/UserService/EventsRestApi.Services.UserService.Infrastructure/"]
COPY ["src/Services/UserService/EventsRestApi.Services.UserService.Presentation/EventsRestApi.Services.UserService.Presentation.csproj", "src/Services/UserService/EventsRestApi.Services.UserService.Presentation/"]

# Восстанавливаем зависимости для главного запускаемого проекта
RUN dotnet restore "src/Services/UserService/EventsRestApi.Services.UserService.Presentation/EventsRestApi.Services.UserService.Presentation.csproj"

# Копируем весь оставшийся исходный код
COPY . .

# Собираем запускаемый WEB-проект (Presentation)
WORKDIR "/src/src/Services/UserService/EventsRestApi.Services.UserService.Presentation"
RUN dotnet build "EventsRestApi.Services.UserService.Presentation.csproj" -c Release -o /app/build

# 2. Этап публикации (Publish Stage)
FROM build AS publish
RUN dotnet publish "EventsRestApi.Services.UserService.Presentation.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 3. Финальный runtime-образ (Runtime Stage)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EventsRestApi.Services.UserService.Presentation.dll"]