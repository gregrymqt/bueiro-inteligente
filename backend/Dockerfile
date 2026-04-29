# EstÃ¡gio de Runtime (ProduÃ§Ã£o)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# EstÃ¡gio de Build e Ferramentas
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
	cp -R "$PROJECT_DIR"/. /tmp/backend-work/

WORKDIR /tmp/backend-work
RUN dotnet restore "backend.csproj"
RUN dotnet build "backend.csproj" -c $BUILD_CONFIGURATION -o /app/build

# PublicaÃ§Ã£o da aplicaÃ§Ã£o
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR /tmp/backend-work
RUN dotnet publish "backend.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# EstÃ¡gio Final (Copia apenas o necessÃ¡rio para rodar)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# VariÃ¡vel para o .NET 8 e Render
ENV ASPNETCORE_HTTP_PORTS=8080

ENTRYPOINT ["dotnet", "backend.dll"]
