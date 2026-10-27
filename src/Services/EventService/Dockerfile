# 1. Этап сборки (Build Stage)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Копируем проект SharedContracts и проекты EventService
COPY ["src/Shared/EventsRestApi.Services.SharedContracts/EventsRestApi.Services.SharedContracts.csproj", "src/Shared/EventsRestApi.Services.SharedContracts/"]

COPY ["src/Services/EventService/EventsRestApi.Services.EventService.Domain/EventsRestApi.Services.EventService.Domain.csproj", "src/Services/EventService/EventsRestApi.Services.EventService.Domain/"]
COPY ["src/Services/EventService/EventsRestApi.Services.EventService.Application/EventsRestApi.Services.EventService.Application.csproj", "src/Services/EventService/EventsRestApi.Services.EventService.Application/"]
COPY ["src/Services/EventService/EventsRestApi.Services.EventService.Infrastructure/EventsRestApi.Services.EventService.Infrastructure.csproj", "src/Services/EventService/EventsRestApi.Services.EventService.Infrastructure/"]
COPY ["src/Services/EventService/EventsRestApi.Services.EventService.Presentation/EventsRestApi.Services.EventService.Presentation.csproj", "src/Services/EventService/EventsRestApi.Services.EventService.Presentation/"]

# Восстанавливаем зависимости для главного запускаемого проекта
RUN dotnet restore "src/Services/EventService/EventsRestApi.Services.EventService.Presentation/EventsRestApi.Services.EventService.Presentation.csproj"

# Копируем весь оставшийся исходный код
COPY . .

# Собираем запускаемый WEB-проект (Presentation)
WORKDIR "/src/src/Services/EventService/EventsRestApi.Services.EventService.Presentation"
RUN dotnet build "EventsRestApi.Services.EventService.Presentation.csproj" -c Release -o /app/build

# 2. Этап публикации (Publish Stage)
FROM build AS publish
RUN dotnet publish "EventsRestApi.Services.EventService.Presentation.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 3. Финальный runtime-образ (Runtime Stage)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EventsRestApi.Services.EventService.Presentation.dll"]