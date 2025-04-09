# Etapa de build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copia os arquivos do projeto
COPY . ./

# Restaura os pacotes
RUN dotnet restore

# Publica a aplicação
RUN dotnet publish -c Release -o out

# Etapa final: runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Usa a porta do Railway via variável de ambiente
ENV ASPNETCORE_URLS=http://+:${PORT}

ENTRYPOINT ["dotnet", "OxfordOnline.AppHost.dll"]