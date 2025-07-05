# Используем официальный .NET Runtime (для запуска)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# Используем .NET SDK (для сборки)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем файлы проекта и выполняем restore
COPY ["TG_Fitz.csproj", "./"]
COPY ["appsettings.json", "./"]
RUN dotnet restore "./TG_Fitz.csproj"

# Копируем весь исходный код и собираем проект
COPY . .
RUN dotnet publish "./TG_Fitz.csproj" -c Release -o /app/publish

# Запуск контейнера
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

CMD ["dotnet", "TG_Fitz.dll"]
