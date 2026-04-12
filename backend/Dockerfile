# Estágio de Runtime (Produção)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Estágio de Build e Ferramentas
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Instala o dotnet-ef globalmente para rodar as migrations
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"

# Copia e restaura o projeto (agora renomeado para backend)
COPY ["backend/backend.csproj", "backend/"]
RUN dotnet restore "backend/backend.csproj"

# Copia todo o código e compila
COPY . .
WORKDIR "/src/backend"
RUN dotnet build "backend.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publicação da aplicação
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "backend.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Estágio Final (Copia apenas o necessário para rodar)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Copia o script de entrada e dá permissão de execução
COPY entrypoint.sh /entrypoint.sh
USER root
RUN chmod +x ./entrypoint.sh

# O entrypoint gerencia migrations e o início da app
ENTRYPOINT ["/entrypoint.sh"]