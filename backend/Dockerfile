# Estágio 1: Builder (Instalação de dependências pesadas)
FROM python:3.11-slim as builder

WORKDIR /app

ENV PYTHONDONTWRITEBYTECODE=1
ENV PYTHONUNBUFFERED=1

# Instala gcc e dependências de sistema necessárias para compilar libs (ex: psycopg2)
RUN apt-get update && apt-get install -y --no-install-recommends gcc python3-dev libpq-dev

COPY requirements.txt .
RUN pip install --upgrade pip && \
    pip install --user --no-cache-dir -r requirements.txt

# Estágio 2: Runtime (Imagem final de execução)
FROM python:3.11-slim

WORKDIR /app

# Copia apenas as libs instaladas no builder (pasta .local)
COPY --from=builder /root/.local /root/.local
COPY . .

# Garante que o Python encontre as dependências e não gere arquivos .pyc
ENV PATH=/root/.local/bin:$PATH
ENV PYTHONDONTWRITEBYTECODE=1
ENV PYTHONUNBUFFERED=1

# Configuração do Script de Entrada (Alembic Migrations + Uvicorn)
COPY entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh

# Porta padrão do backend
EXPOSE 8000

ENTRYPOINT ["/entrypoint.sh"]