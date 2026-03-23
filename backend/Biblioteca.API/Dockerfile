FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["backend/Biblioteca.API/Biblioteca.API.csproj", "backend/Biblioteca.API/"]
COPY ["backend/Biblioteca.Application/Biblioteca.Application.csproj", "backend/Biblioteca.Application/"]
COPY ["backend/Biblioteca.Domain/Biblioteca.Domain.csproj", "backend/Biblioteca.Domain/"]
COPY ["backend/Biblioteca.Infrastructure/Biblioteca.Infrastructure.csproj", "backend/Biblioteca.Infrastructure/"]
COPY ["backend/Biblioteca.Persistence/Biblioteca.Persistence.csproj", "backend/Biblioteca.Persistence/"]

RUN dotnet restore "backend/Biblioteca.API/Biblioteca.API.csproj"

COPY . .

RUN dotnet publish "backend/Biblioteca.API/Biblioteca.API.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false \
    /p:SkipDatabaseInitOnBuild=true

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "Biblioteca.API.dll"]
