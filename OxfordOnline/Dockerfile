# Etapa 1 - Build com SDK do .NET 8
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia os arquivos do projeto
COPY . .

# Restaura dependências
RUN dotnet restore "OxfordOnline.AppHost/OxfordOnline.AppHost.csproj"

# Publica a aplicação em modo release
RUN dotnet publish "OxfordOnline.AppHost/OxfordOnline.AppHost.csproj" -c Release -o /app/publish

# Etapa 2 - Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copia a publicação
COPY --from=build /app/publish .

# Railway usa a variável PORT automaticamente
ENV ASPNETCORE_URLS=http://+:${PORT}

# Inicia a aplicação
ENTRYPOINT ["dotnet", "OxfordOnline.AppHost.dll"]