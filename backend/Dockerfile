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

# Copia o contexto inteiro e normaliza o caminho do projeto para /tmp/backend-work
COPY . .
RUN set -eux; \
	if [ -f /src/backend/backend.csproj ]; then \
		PROJECT_DIR=/src/backend; \
	elif [ -f /src/backend.csproj ]; then \
		PROJECT_DIR=/src; \
	else \
		echo "backend.csproj not found in build context"; \
		exit 1; \
	fi; \
	rm -rf /tmp/backend-work; \
	mkdir -p /tmp/backend-work; \
	cp -R "$PROJECT_DIR"/. /tmp/backend-work/; \
	if [ ! -f /tmp/backend-work/entrypoint.sh ]; then \
		echo "entrypoint.sh not found in build context"; \
		exit 1; \
	fi

WORKDIR /tmp/backend-work
RUN dotnet restore "backend.csproj"
RUN dotnet build "backend.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publicação da aplicação
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR /tmp/backend-work
RUN dotnet publish "backend.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Estágio Final (Copia apenas o necessário para rodar)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Copia o script de entrada e dá permissão de execução
COPY --from=build /tmp/backend-work/entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh

# O entrypoint gerencia migrations e o início da app
ENTRYPOINT ["/entrypoint.sh"]