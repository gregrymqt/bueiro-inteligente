# Estágio de Build para o código do ESP32 usando Arduino CLI
# Este Dockerfile é útil para integração contínua (CI/CD) para garantir que
# o código do microcontrolador sempre compila com sucesso.

FROM ubuntu:22.04

# Instala as dependências de sistema para o Arduino CLI e o Core do ESP32
RUN apt-get update && apt-get install -y \
    curl \
    python3 \
    python3-pip \
    git \
    python3-serial \
    && rm -rf /var/lib/apt/lists/*

# Instala o Arduino CLI
RUN curl -fsSL https://raw.githubusercontent.com/arduino/arduino-cli/master/install.sh | BINDIR=/usr/local/bin sh

# Configura o Arduino CLI para baixar os pacotes da plataforma ESP32
RUN arduino-cli config init && \
    arduino-cli config set board_manager.additional_urls https://raw.githubusercontent.com/espressif/arduino-esp32/gh-pages/package_esp32_index.json

# Atualiza as fontes e instala o núcleo/core do ESP32
RUN arduino-cli core update-index && \
    arduino-cli core install esp32:esp32

# Instala as dependências externas do seu .ino
# No seu caso: ArduinoJson
RUN arduino-cli lib install "ArduinoJson"

# Define o espaço de trabalho onde o código do projeto será armazenado
WORKDIR /workspace

# Copia os arquivos da sua pasta para o ambiente do contêiner
COPY . .

# O comando padrão quando o contêiner rodar será compilar o arquivo .ino.
# Isso valida o código contra o compilador do ESP32.
# Ele verificará erros de sintaxe, falta de imports, etc.
CMD ["arduino-cli", "compile", "--fqbn", "esp32:esp32:esp32", "esp_bueiro.ino"]
