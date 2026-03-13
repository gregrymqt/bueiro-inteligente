FROM python:3.11-slim

WORKDIR /app

COPY requirements.txt .

RUN pip install --no-cache-dir -r requirements.txt

COPY . .

# A instrução CMD especifica o comando a ser executado quando o contêiner for iniciado.
# Assumi que seu arquivo de entrada principal se chama 'main.py'.
# Se o seu arquivo estiver em um subdiretório (ex: app/main.py),
# você deve alterar o comando para: CMD ["python", "app/main.py"]
CMD ["python", "main.py"]