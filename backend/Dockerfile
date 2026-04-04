FROM python:3.11-slim

ENV PYTHONDONTWRITEBYTECODE=1
ENV PYTHONUNBUFFERED=1

WORKDIR /app

# Copia o requirements.txt a partir do contexto do backend
COPY requirements.txt .

RUN pip install --upgrade pip && \
    pip install --no-cache-dir -r requirements.txt

# Copia todo o conteúdo da pasta backend para o /app
COPY . .

# Copia o script de entrada
COPY entrypoint.sh /entrypoint.sh

# Dá permissão de execução para o script
RUN chmod +x /entrypoint.sh

# Define o script como o comando principal
ENTRYPOINT ["/entrypoint.sh"]