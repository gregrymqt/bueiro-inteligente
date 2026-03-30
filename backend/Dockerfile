FROM python:3.11-slim

WORKDIR /app

# Copia o requirements.txt a partir do contexto do backend
COPY requirements.txt .

RUN pip install --upgrade pip
RUN pip install --no-cache-dir -r requirements.txt

# Copia todo o conteúdo da pasta backend para o /app
COPY . .

# Comando para iniciar o servidor Uvicorn (FastAPI) rodando o arquivo app/main.py
CMD ["uvicorn", "app.main:app", "--host", "0.0.0.0", "--port", "8000"]
