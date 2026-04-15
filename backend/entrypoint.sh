#!/bin/bash

# Aborta se algum comando falhar
set -e

echo "🚀 [BACKEND] Iniciando rotina de inicialização..."

echo "✅ A API cuidará das migrations automaticamente via Entity Framework (MigrateAsync)."
echo "✅ Subindo API .NET 8..."

# O Render injeta a variável $PORT. Se não houver, usa 8080 por padrão.
exec dotnet backend.dll --urls "http://0.0.0.0:${PORT:-8080}"