# Bueiro Inteligente

Ecossistema distribuído para monitoramento preventivo de bueiros urbanos, combinando backend em ASP.NET Core, frontend em React, app Android e firmware para ESP32.

## Visão Geral

O projeto foi desenhado para receber leituras dos sensores, processar os dados no backend, persistir histórico, emitir eventos em tempo real e alimentar as interfaces web, mobile e embarcada.

## Stack

- **Backend:** ASP.NET Core 8, C#, Entity Framework Core, Npgsql, Redis, Quartz e SignalR.
- **Frontend Web:** React 19, Vite, TypeScript e SCSS.
- **Mobile:** Android nativo em Kotlin, com minSdk 24.
- **Hardware:** ESP32/ESP8266 em C++ com Arduino IDE.
- **Infraestrutura:** Docker e Docker Compose.

## Estrutura do Repositório

- [backend/](backend/) - API principal, features, persistência e hubs em tempo real.
- [frontend/](frontend/) - Portal web em React.
- [app/](app/) - Aplicativo Android.
- [hardware/esp_bueiro/](hardware/esp_bueiro/) - Código embarcado do sensor.
- [Tests/](Tests/) - Suite de testes automatizados do backend.

## Como Executar

### 1. Configurar o ambiente

Crie um arquivo [.env](.env) na raiz do repositório com as variáveis do projeto. O backend carrega esse arquivo automaticamente e o `docker-compose.yml` também injeta os valores no container.

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
- O banco local usa `DB_LOCAL=true` e `DATABASE_URL_LOCAL`.
- O serviço de tempo real usa `SignalR` em `/realtime/ws`.
- Mantenha os arquivos `.env` fora do controle de versão.
