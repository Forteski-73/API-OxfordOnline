FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Instalar o workload Aspire
RUN dotnet workload restore

# Copiar o projeto
COPY . .

# Restaurar pacotes
RUN dotnet restore

# Compilar o projeto
RUN dotnet build -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Copiar a aplicação compilada
COPY --from=build /app .

ENTRYPOINT ["dotnet", "OxfordOnline.AppHost.dll"]