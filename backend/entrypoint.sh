#!/bin/bash

# Aborta o script se algum comando falhar
set -e

echo "🚀 Iniciando as migrações do banco de dados..."
alembic upgrade head

echo "✅ Migrações concluídas. Subindo a API..."
# Aqui você coloca o comando que já estava no seu CMD do Dockerfile
exec uvicorn app.main:app --host 0.0.0.0 --port $PORT 