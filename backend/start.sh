#!/bin/bash
# Interrompe o script imediatamente se houver algum erro
set -e

# Verifica se está rodando dentro do Docker ou no ambiente local (Linux/Windows)
if [ -f /.dockerenv ]; then
    echo "🐳 Ambiente detectado: Docker (Container Linux)"
else
    if [[ "$OSTYPE" == "linux-gnu"* ]]; then
        echo "🐧 Ambiente detectado: Linux Local"
    elif [[ "$OSTYPE" == "msys"* || "$OSTYPE" == "cygwin"* || "$OSTYPE" == "win32"* ]]; then
        echo "🪟 Ambiente detectado: Windows Local (Git Bash / MinGW)"
    else
        echo "💻 Ambiente detectado: Outro ($OSTYPE)"
    fi
    echo "⚠️ Aviso: O script está sendo executado fora do contêiner Docker."
fi

echo "Executando migrations do banco de dados (Alembic)..."
alembic upgrade head

echo "Iniciando a API FastAPI (Uvicorn)..."
exec uvicorn app.main:app --host 0.0.0.0 --port 8000 --reload
