#!/bin/bash

# Aborta se algum comando falhar
set -e

echo "🚀 [BACKEND] Iniciando rotina de inicialização..."

# Tenta aplicar as migrations. 
# Nota: Em produção (Render), se você usar uma imagem de runtime pura, 
# o comando 'dotnet ef' não funcionará. Recomendo chamar 
# 'context.Database.Migrate()' no seu Program.cs para garantir 100% em Prod.
if [ "$ASPNETCORE_ENVIRONMENT" = "Development" ]; then
    echo "📂 [DEV] Aplicando migrations do EF Core..."
    dotnet ef database update
fi

echo "✅ Banco de dados verificado. Subindo API .NET 8..."

# O Render injeta a variável $PORT. Se não houver, usa 8080 por padrão.
exec dotnet backend.dll --urls "http://0.0.0.0:${PORT:-8080}"