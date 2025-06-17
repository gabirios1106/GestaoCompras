# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080 8081

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["GestaoCompras.API/GestaoCompras.API.csproj", "GestaoCompras.API/"]
COPY ["GestaoCompras.DTO/GestaoCompras.DTO.csproj", "GestaoCompras.DTO/"]
COPY ["GestaoCompras.Enums/GestaoCompras.Enums.csproj", "GestaoCompras.Enums/"]
COPY ["GestaoCompras.Utils/GestaoCompras.Utils.csproj", "GestaoCompras.Utils/"]
COPY ["GestaoCompras.Models/GestaoCompras.Models.csproj", "GestaoCompras.Models/"]
RUN dotnet restore "./GestaoCompras.API/GestaoCompras.API.csproj"
COPY . .
WORKDIR "/src/GestaoCompras.API"
RUN dotnet build "./GestaoCompras.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./GestaoCompras.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
USER root
RUN mkdir -p /home/app/.aspnet/https
USER app
WORKDIR /app
COPY --from=publish /app/publish .
USER app
ENTRYPOINT ["dotnet", "GestaoCompras.API.dll"]