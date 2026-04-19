# Bueiro Inteligente - Hardware ESP32

Este diretório contém o sketch embarcado do projeto, em `esp_bueiro/esp_bueiro.ino`, que lê sensores, monta um payload JSON enxuto e envia medições para a API do backend.

## Stack do Sketch

- `WiFi.h` para conexão de rede
- `HTTPClient.h` para envio HTTP
- `ArduinoJson.h` para serialização do payload
- `secrets.h` para credenciais locais de Wi-Fi e token do hardware

## Fluxo de Comunicação

1. O dispositivo conecta na rede Wi-Fi definida em `secrets.h`.
2. O sensor mede a distância interna do bueiro.
3. O sketch monta um `StaticJsonDocument<200>` com os dados da leitura.
4. A requisição é enviada para `POST /monitoring/medicoes?token=...`.
5. O backend valida o token do hardware, persiste a medição e pode emitir atualização em tempo real.

Para evitar envio redundante, o firmware só publica uma nova leitura quando a diferença em relação à última amostra confirmada ultrapassa 2 cm. Mesmo assim, o dispositivo mantém um heartbeat a cada 5 minutos para sinalizar que segue online, preservar visibilidade operacional e evitar longos períodos sem comunicação com a API.

## Configuração Local

Antes de compilar, ajuste os valores abaixo em `secrets.h`:

- `WIFI_SSID`
- `WIFI_PASS`
- `HARDWARE_TOKEN`
- `API_URL`

## Compilação

O repositório já inclui um `Dockerfile` e um perfil no `docker-compose.yml` para validar o sketch com Arduino CLI.

```bash
docker compose --profile tools run --rm hardware
```

Se preferir a Arduino IDE, abra o projeto `esp_bueiro/` e garanta a instalação da biblioteca `ArduinoJson`.