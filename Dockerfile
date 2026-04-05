# Сборка приложения
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Копируем файл проекта и восстанавливаем зависимости
COPY ["ReddirWebApi.csproj", "./"]
RUN dotnet restore "./ReddirWebApi.csproj"

# Копируем весь остальной код и собираем релизную версию
COPY . .
RUN dotnet publish "ReddirWebApi.csproj" -c Release -o /app/publish

# Запуск приложения (легкий образ)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080

# Копируем собранные файлы из первого этапа
COPY --from=build /app/publish .

# Запускаем API
ENTRYPOINT ["dotnet", "ReddirWebApi.dll"]

