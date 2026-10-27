# 1. Этап сборки (Build Stage)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Копируем проект SharedContracts и проекты BookingService
COPY ["src/Shared/EventsRestApi.Services.SharedContracts/EventsRestApi.Services.SharedContracts.csproj", "src/Shared/EventsRestApi.Services.SharedContracts/"]

COPY ["src/Services/BookingService/EventsRestApi.Services.BookingService.Domain/EventsRestApi.Services.BookingService.Domain.csproj", "src/Services/BookingService/EventsRestApi.Services.BookingService.Domain/"]
COPY ["src/Services/BookingService/EventsRestApi.Services.BookingService.Application/EventsRestApi.Services.BookingService.Application.csproj", "src/Services/BookingService/EventsRestApi.Services.BookingService.Application/"]
COPY ["src/Services/BookingService/EventsRestApi.Services.BookingService.Infrastructure/EventsRestApi.Services.BookingService.Infrastructure.csproj", "src/Services/BookingService/EventsRestApi.Services.BookingService.Infrastructure/"]
COPY ["src/Services/BookingService/EventsRestApi.Services.BookingService.Presentation/EventsRestApi.Services.BookingService.Presentation.csproj", "src/Services/BookingService/EventsRestApi.Services.BookingService.Presentation/"]

# Восстанавливаем зависимости для главного запускаемого проекта
RUN dotnet restore "src/Services/BookingService/EventsRestApi.Services.BookingService.Presentation/EventsRestApi.Services.BookingService.Presentation.csproj"

# Копируем весь оставшийся исходный код
COPY . .

# Собираем запускаемый WEB-проект (Presentation)
WORKDIR "/src/src/Services/BookingService/EventsRestApi.Services.BookingService.Presentation"
RUN dotnet build "EventsRestApi.Services.BookingService.Presentation.csproj" -c Release -o /app/build

# 2. Этап публикации (Publish Stage)
FROM build AS publish
RUN dotnet publish "EventsRestApi.Services.BookingService.Presentation.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 3. Финальный runtime-образ (Runtime Stage)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EventsRestApi.Services.BookingService.Presentation.dll"]