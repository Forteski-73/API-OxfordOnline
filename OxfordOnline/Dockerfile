# Use uma imagem base com o SDK do .NET
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Defina o diretório de trabalho para o contêiner
WORKDIR /src

# Copie o arquivo .csproj para dentro do contêiner (ajustado para a pasta correta)
COPY OxfordOnline.csproj ./

# Restaure as dependências do projeto
RUN dotnet restore

# Copie o restante do código para dentro do contêiner
COPY . .

# Publique a aplicação para a pasta /app
RUN dotnet publish -c Release -o /app

# Use uma imagem base do ASP.NET para execução
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# Defina o diretório de trabalho para o contêiner
WORKDIR /app

# Copie a aplicação publicada do contêiner de build
COPY --from=build /app .

# Defina a porta e inicie a aplicação
EXPOSE 80
ENTRYPOINT ["dotnet", "OxfordOnline.dll"]
