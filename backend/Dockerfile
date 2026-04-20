# Estágio de Runtime (Produção)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Estágio de Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# 1. Copia o .csproj para restaurar dependências (otimiza o cache do Docker)
# O caminho deve ser relativo à raiz do repositório
COPY ["backend/backend.csproj", "backend/"]
RUN dotnet restore "backend/backend.csproj"

# 2. Copia todo o conteúdo da pasta backend para o container
COPY ["backend/", "backend/"]

# 3. Define o diretório de trabalho onde o arquivo de projeto realmente está
WORKDIR "/src/backend"
RUN dotnet build "backend.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Estágio de Publicação
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "backend.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Estágio Final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Variável para o .NET 8 e Render
ENV ASPNETCORE_HTTP_PORTS=8080

ENTRYPOINT ["dotnet", "backend.dll"]