FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY TaskManagerAPI.sln ./
COPY TaskManagerAPI.Core/TaskManagerAPI.Core.csproj TaskManagerAPI.Core/
COPY TaskManagerAPI.Infrastructure/TaskManagerAPI.Infrastructure.csproj TaskManagerAPI.Infrastructure/
COPY TaskManagerAPI.API/TaskManagerAPI.API.csproj TaskManagerAPI.API/
RUN dotnet restore TaskManagerAPI.sln

COPY . .
RUN dotnet publish TaskManagerAPI.API/TaskManagerAPI.API.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

ENTRYPOINT ["sh", "-c", "dotnet TaskManagerAPI.API.dll --urls http://0.0.0.0:${PORT:-8080}"]
