# Estágio de Runtime (Produção)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Estágio de Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# 1. Copia APENAS o arquivo de projeto usando o caminho correto a partir da raiz
COPY ["backend/backend.csproj", "backend/"]
RUN dotnet restore "backend/backend.csproj"

# 2. Copia todo o resto do código da pasta backend
COPY ["backend/", "backend/"]

# 3. Muda o diretório de trabalho para dentro da pasta do C# para compilar
WORKDIR /src/backend
RUN dotnet build "backend.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publicação da aplicação
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "backend.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Estágio Final (Imagem enxuta apenas com o necessário para rodar)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# O .NET 8 usa essa variável de ambiente para expor a porta local e no Render
ENV ASPNETCORE_HTTP_PORTS=8080

# Roda o C# diretamente, sem script shell
ENTRYPOINT ["dotnet", "backend.dll"]