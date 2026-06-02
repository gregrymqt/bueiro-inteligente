# Bueiro Inteligente

Ecossistema distribuído para monitoramento preventivo de bueiros urbanos, combinando backend em ASP.NET Core, frontend em React, app Android e firmware para ESP32.

## Visão Geral

O projeto foi desenhado para receber leituras dos sensores, processar os dados no backend, persistir histórico, emitir eventos em tempo real e alimentar as interfaces web, mobile e embarcada. A telemetria do hardware (ESP32/ESP8266) pode passar opcionalmente pelo Node-RED ou por um broker MQTT (Mosquitto) antes de ser processada via HTTP e distribuída em tempo real via SignalR.

## Stack

- **Backend:** ASP.NET Core 8, C#, Entity Framework Core, Npgsql, Redis, Quartz e SignalR.
- **Frontend Web:** React 19, Vite, TypeScript e SCSS.
- **Mobile:** Android nativo em Kotlin, com minSdk 24.
- **Hardware:** ESP32/ESP8266 em C++ com Arduino IDE.
- **Infraestrutura:** Docker, Docker Compose, PostgreSQL, Redis e Mosquitto (Broker MQTT).

### Integrações e Serviços de Terceiros
- **Supabase:** Armazenamento de arquivos (Storage).
- **Mercado Pago:** Pagamentos e gestão de assinaturas.
- **Rows API:** Integração e exportação de dados para planilhas.
- **Google OAuth & Firebase AdminSDK:** Autenticação unificada de usuários.
- **Node-RED:** Roteamento e processamento opcional de telemetria de hardware antes de chegar ao backend.

## Estrutura do Repositório

- [backend/](backend/) - API principal, features, persistência e hubs em tempo real.
- [frontend/](frontend/) - Portal web em React.
- [app/](app/) - Aplicativo Android.
- [hardware/esp_bueiro/](hardware/esp_bueiro/) - Código embarcado do sensor.
- [Tests/](Tests/) - Suite de testes automatizados do backend.

## Como Executar

### 1. Configurar o ambiente

Crie um arquivo [.env](.env) na raiz do repositório baseado no `.env.example` com as variáveis do projeto. O backend carrega esse arquivo automaticamente e o `docker-compose.yml` também injeta os valores no container. 

> **⚠️ Aviso:** Além das configurações de banco de dados, certifique-se de preencher as chaves das integrações externas (Mercado Pago, Supabase, Rows API, Google OAuth e Node-RED) para garantir o funcionamento completo das features da aplicação.

### 2. Subir backend, banco e Redis

```bash
docker compose up -d --build
```

Isso sobe o backend, PostgreSQL e Redis. O backend fica exposto em `http://localhost:8080`.

### 3. Rodar cada parte separadamente

```bash
cd backend
dotnet run
```

```bash
cd Tests
dotnet test backend.Tests.csproj
```

```bash
cd frontend
npm install
npm run dev
```

No Android, abra a pasta [app/](app/) no Android Studio ou use `./gradlew assembleDebug`.

No hardware, use a pasta [hardware/esp_bueiro/](hardware/esp_bueiro/) na Arduino IDE.

## Documentação Útil

- [README do backend](backend/README-BACKEND.md)
- [README dos testes](Tests/README-TEST-BACKEND.md)
- [Instruções do Copilot](.github/copilot-instructions.md)

## Notas

- O backend segue a arquitetura `Controller -> Service -> Repository`.
- O banco local usa `ConnectionStrings__DefaultConnection` e `ConnectionStrings__MigrationsConnection`; eles precisam apontar para o PostgreSQL ativo no ambiente, seja via Docker Compose ou via banco local.
- O serviço de tempo real usa `SignalR` em `/realtime/ws`.
- Mantenha os arquivos `.env` fora do controle de versão.
