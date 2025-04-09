# Etapa de build - usa SDK do .NET 8.0
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copia tudo
COPY . ./

# Restaura dependências
RUN dotnet restore

# Publica para produção
RUN dotnet publish OxfordOnline.AppHost/OxfordOnline.AppHost.csproj -c Release -o /app/publish

# Etapa final - runtime do .NET 8.0
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Usa a porta padrão do Railway
ENV ASPNETCORE_URLS=http://+:${PORT}

ENTRYPOINT ["dotnet", "OxfordOnline.AppHost.dll"]