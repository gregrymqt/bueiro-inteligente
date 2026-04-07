#!/bin/bash

# Aborta o script se algum comando falhar
set -e

echo "🚀 Verificando migrações do banco de dados..."
# O Alembic só aplicará mudanças se houver arquivos novos na pasta /alembic/versions
alembic upgrade head

echo "✅ Banco de dados sincronizado. Subindo a API..."
# O Render injeta a variável $PORT automaticamente[cite: 11, 15]
exec uvicorn app.main:app --host 0.0.0.0 --port $PORT