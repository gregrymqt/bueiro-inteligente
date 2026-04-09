# 🌊 Bueiro Inteligente (Smart Drain)

Bem-vindo ao repositório do projeto **Bueiro Inteligente**! 
Este é um ecossistema distribuído focado no monitoramento e gestão preventiva de bueiros urbanos e drenagem pluvial, combinando hardware embarcado (IoT) e plataformas de software (Web e Mobile).

---

## 🎯 O Que Este Projeto Faz? (Regra de Negócio)

O sistema foi desenhado para evitar enchentes e otimizar rotas de manutenção urbana. O fluxo principal funciona assim:

1. **Coleta (IoT):** Dispositivos usando **ESP32** com sensores medem o nível de água e/ou obstrução (lixo/resíduos) dentro de um bueiro em intervalos regulares.
2. **Transmissão:** Os dados são enviados via rede Wi-Fi/HTTP por meio de um payload JSON para o nosso **Backend**.
3. **Processamento & ETL:** O Backend analisa as leituras. Se níveis críticos forem detectados, ele processa a lógica de notificação, armazena o histórico no banco de dados, utiliza cache para alta performance e dispara alertas via WebSockets. Periodicamente, as métricas também podem ser sincronizadas com planilhas (como Rows.com) usando processos em background.
4. **Visualização:** Agentes e gestores podem visualizar os alertas e mapas tanto no **Portal Web** quanto no **App Mobile**.

---

## 🛠️ Stack Tecnológica

- **Backend:** Python 3.x, FastAPI (Uvicorn), PostgreSQL (com SQLAlchemy e Alembic), Redis, APScheduler, WebSockets, HTTPX.
- **Frontend Web:** React 19, Vite, TypeScript, UI componetizada (SCSS Modules).
- **Mobile:** Android Nativo (Kotlin, Min SDK 24, MVVM, Clean Architecture).
- **IoT / Hardware:** ESP32/ESP8266, C++, ArduinoIDE, ArduinoJson.
- **Infraestrutura:** Docker e Docker Compose.

> 🧠 **Nota para Assistentes e IAs:** Para detalhes de padrões de arquitetura de código, regras de injeção de dependência e limites de módulos, consulte o arquivo [`.github/copilot-instructions.md`](.github/copilot-instructions.md).

---

## 🧭 Arquitetura e Integração

O ecossistema é centrado no backend FastAPI. Cada frente conversa com ele por contratos específicos:

1. **Hardware (ESP32/ESP8266)** envia leituras para `POST /monitoring/medicoes?token=...` com payload JSON contendo `id_bueiro`, `distancia_cm`, `latitude` e `longitude`.
2. **Backend (Python)** valida o hardware token, persiste a medição no PostgreSQL, atualiza o Redis, emite eventos em tempo real via WebSocket em `/realtime/ws` e agenda a sincronização ETL com Rows.
3. **Frontend Web (React)** consome `GET /home`, `GET /auth/users/me`, `GET /monitoring/{id}/status` e o WebSocket de realtime; a comunicação HTTP passa pelo `ApiClient` e os alertas visuais passam pelo `AlertService`.
4. **Mobile (Kotlin)** consome os mesmos contratos REST e o canal de realtime, mantendo estado de interface via `StateFlow` e `collectAsStateWithLifecycle()`.

---

## 🚀 Como Rodar o Projeto Localmente

Nós utilizamos o **Docker Compose** para orquestrar todos os serviços localmente, garantindo que tudo rode de maneira contínua e sem poluir o seu sistema.

### 1. Pré-Requisitos
- [Docker](https://docs.docker.com/get-docker/) e [Docker Compose](https://docs.docker.com/compose/install/) instalados na sua máquina.
- Um banco de dados PostgreSQL rodando (local ou na nuvem).

### 2. Configuração do Ambiente (.env)
Na raiz do projeto, crie um arquivo chamado `.env` e preencha as variáveis usadas pelo backend e pelo Docker Compose:

```env
# Backend / Banco / Redis
DATABASE_URL_LOCAL=postgresql+asyncpg://bueiro_user:bueiro_password@db:5432/bueiro_db
DATABASE_URL_CLOUD=
DB_LOCAL=true
REDIS_URL=redis://redis:6379/0
REDIS_LOCAL=true

# Segurança e integrações
SECRET_KEY=troque-esta-chave
HARDWARE_TOKEN=token-do-esp32
ROWS_API_KEY=
ROWS_SPREADSHEET_ID=
ROWS_TABLE_ID=

# Executável local do Uvicorn (opcional)
HOST=0.0.0.0
PORT=8000
```

Se você for executar o backend diretamente fora do Docker Compose, replique as mesmas chaves em `backend/.env`, porque o serviço também carrega esse arquivo por padrão.

### 3. Iniciar os Serviços Web (API + Portal Web + PostgreSQL + Redis)
Basta rodar o comando no terminal a partir da raiz do projeto:

```bash
docker compose up -d --build
```
Isso iniciará:
- **Backend (FastAPI):** Acessível em `http://localhost:8000` (Acesse `http://localhost:8000/docs` para ver o Swagger). O Hot-reload de desenvolvimento está ativado por padrão.
- **Frontend (React/Vite):** Acessível em `http://localhost:5173`.
- **PostgreSQL:** Banco relacional exposto na porta `5432`.
- **Cache (Redis):** Integrado e isolado na porta `6379`.

### 4. Usar Ferramentas de Build (Mobile e IoT)

Não precisa instalar o Android Studio ou o Arduino CLI nativamente apenas para compilar. Utilizamos _profiles_ do Docker.

- **Para compilar o APK do Aplicativo Android:**
  ```bash
  docker compose --profile tools run --rm app-mobile
  ```
  *(O APK final ficará na pasta `app/app/build/outputs/apk/debug/app-debug.apk` ou similar no seu computador)*

- **Para validar/compilar o código do Hardware (ESP32):**
  ```bash
  docker compose --profile tools run --rm hardware
  ```
  O sketch usa `secrets.h` para credenciais locais e autentica com query token na rota do backend.

---

## 📁 Estrutura de Pastas

- `/backend/`: Lógica da API, separada na arquitetura de Features (Controllers, Services, Repositories).
- `/frontend/`: Portal gestor (Dashboard) em React SPA.
- `/app/`: Aplicativo nativo Android que os agentes levam para a rua.
- `/hardware/esp_bueiro/`: O código embarcado compilável rodando em loop nos pontos físicos de monitoramento. A documentação específica fica em [`hardware/README-HARDWARE.md`](hardware/README-HARDWARE.md).

---

## 🤝 Contribuindo
Ao contribuir, lembre-se de seguir os padrões estritos de arquitetura detalhados neste repositório: regras de negócio separadas (não use BD direto em interfaces) e sempre trabalhe com injeção de dependências e Modelos de Validação (Pydantic / TS Interfaces).
