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

## 🚀 Como Rodar o Projeto Localmente

Nós utilizamos o **Docker Compose** para orquestrar todos os serviços localmente, garantindo que tudo rode de maneira contínua e sem poluir o seu sistema.

### 1. Pré-Requisitos
- [Docker](https://docs.docker.com/get-docker/) e [Docker Compose](https://docs.docker.com/compose/install/) instalados na sua máquina.
- Um banco de dados PostgreSQL rodando (local ou na nuvem).

### 2. Configuração do Ambiente (.env)
Na raiz do projeto, crie um arquivo chamado `.env` e preencha com as suas credenciais do banco de dados PostgreSQL:

```env
# Requisito Obrigatório: Conexão com o Banco de Dados PostgreSQL
DATABASE_URL=postgresql://usuario:senha@localhost:5432/nome_do_banco

# Outras variáveis (Opcionais / Defaults)
HOST=0.0.0.0
PORT=8000
```

### 3. Iniciar os Serviços Web (API + Portal Web + Banco Redis)
Basta rodar o comando no terminal a partir da raiz do projeto:

```bash
docker compose up -d --build
```
Isso iniciará:
- **Backend (FastAPI):** Acessível em `http://localhost:8000` (Acesse `http://localhost:8000/docs` para ver o Swagger). O Hot-reload de desenvolvimento está ativado por padrão.
- **Frontend (React/Nginx):** Acessível em `http://localhost:8080`.
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

---

## 📁 Estrutura de Pastas

- `/backend/`: Lógica da API, separada na arquitetura de Features (Controllers, Services, Repositories).
- `/frontend/`: Portal gestor (Dashboard) em React SPA.
- `/app/`: Aplicativo nativo Android que os agentes levam para a rua.
- `/hardware/esp_bueiro/`: O código embarcado compilável rodando em loop nos pontos físicos de monitoramento.

---

## 🤝 Contribuindo
Ao contribuir, lembre-se de seguir os padrões estritos de arquitetura detalhados neste repositório: regras de negócio separadas (não use BD direto em interfaces) e sempre trabalhe com injeção de dependências e Modelos de Validação (Pydantic / TS Interfaces).
