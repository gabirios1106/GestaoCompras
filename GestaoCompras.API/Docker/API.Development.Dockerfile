FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER root
RUN mkdir -p /opt/api/logs
RUN chmod 777 /opt/api/logs
USER app
WORKDIR /app
EXPOSE 8080 8081
ENTRYPOINT ["dotnet", "GestaoCompras.API.dll"]
