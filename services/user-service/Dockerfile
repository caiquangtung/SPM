FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["user-service.csproj", "./"]
RUN dotnet restore "user-service.csproj"

# Copy everything else and build
COPY . .
WORKDIR /src
RUN dotnet build "user-service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "user-service.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "user-service.dll"]

