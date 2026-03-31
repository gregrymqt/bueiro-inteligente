# Contexto Geral do Projeto: Bueiro Inteligente (Smart Drain)

Este é um ecossistema distribuído (IoT, Mobile, Web Frontend e Backend) que monitoriza o estado de bueiros inteligentes e sincroniza dados (ETL). O projeto adota uma arquitetura modular baseada em "Features" (funcionalidades) em suas camadas de software, separando responsabilidades e facilitando a manutenção.

## 🛠 Stack Tecnológica

### Backend (Python)
- **Framework Principal:** FastAPI (com Uvicorn)
- **Linguagem:** Python 3.x
- **Bancos de Dados/Cache:** PostgreSQL (SQLAlchemy + Alembic) e Redis
- **Autenticação:** JWT (python-jose, passlib)
- **Validação de Dados:** Pydantic / Pydantic Settings
- **Agendamento (Jobs/Workers):** APScheduler
- **Tempo Real:** WebSockets
- **Comunicações HTTP (Assíncronas):** HTTPX (Usado para integrações, ex: Adafruit, Rows.com)
- **Estrutura/Padrões:**
  - Arquitetura baseada em Features (`app/features/`).
  - Princípios de Inversão de Dependência (Dependency Injection via interfaces).
  - Padrão Repository e Services para isolar regras de negócio.

### Frontend (React + TypeScript)
- **Framework Principal:** React 19 (criado com Vite)
- **Linguagem:** TypeScript
- **Roteamento:** React Router DOM (v7)
- **Estilização:** SCSS / CSS Modules (Padrão: nomes de arquivos como `Component.scss` ou `Component.module.scss`)
- **Gestão de Estado/Hooks:** Custom Hooks por feature (`useAuth.ts`, `useHomeCarousel.ts`, `useDrainStatus.ts`)
- **Estrutura/Padrões:**
  - Arquitetura baseada em Features (`src/feature/`). Cada feature tem subpastas (`components`, `hooks`, `services`, `types`).
  - Serviços de HTTP isolados (`src/core/http/ApiClient.ts`, `TokenService.ts`, e interceptors como `AuthInterceptor.tsx`).
  - Componentização modular (`src/components/`, `src/pages/`, `src/router/`).

### Mobile (Android)
- **Plataforma:** Android (SDK 24 ao 36+)
- **Linguagem:** Kotlin / Java 11
- **Construção:** Gradle Scripts (`build.gradle.kts`)
- **Estrutura:** Segue os padrões nativos do Android (Pacote base `br.edu.fatecpg`).

### Hardware (IoT)
- **Microcontrolador:** ESP32 / ESP8266
- **Linguagem:** C++ (Arduino IDE)
- **Comunicação:** `WiFi.h`, `HTTPClient.h`
- **Manipulação de Dados:** `ArduinoJson.h`

---

## 📁 Estrutura de Diretórios e Padrões Arquiteturais

### Padrões do Backend (`/backend/app`)
- `/core`: Configurações globais e de ambiente (`config.py`).
- `/extensions`: Inicialização de infraestrutura como banco de dados/cache (`infrastructure.py`), segurança (`auth.py`), websockets (`realtime.py`) e agendamentos (`scheduler.py`).
- `/features`: Módulos de negócio isolados. Cada feature (como `auth`, `monitoring`, `rows`, `cache`, `realtime`) pode conter:
  - `controller.py`: Endpoints do FastAPI (Rotas).
  - `service.py` ou pasta `/services/`: Regras de negócio da aplicação (quando complexa, a feature divide serviços em arquivos, ex: `broadcast_service.py` em `monitoring`).
  - `repository.py`: Interação direta com a camada de dados (PostgreSQL/Redis).
  - `interfaces.py`: Classes abstratas / Tipagens para injeção de dependência.
  - `dto.py` / `dtos.py`: Modelos Pydantic para validação de entrada/saída (Schemas).
- `/routes`: Pasta destinada à agregação e registro das rotas dos controllers.

### Padrões do Frontend (`/frontend/src`)
- `/core`: Utilitários centrais como clientes HTTP base, interceptors e manipulação de tokens (`ApiClient.ts`, `TokenService.ts`, `AuthInterceptor.tsx`).
- `/feature`: Funcionalidades e regras de negócio isoladas (ex: `auth`, `home`, `monitoring`). Dentro de cada feature, dividimos em `/components`, `/hooks`, `/services` e `/types`.
- `/components`: Componentes reutilizáveis distribuídos em interface (`ui/`, como `Card`, `Carousel`) e estruturais de layout (`layout/`, como `Navbar`, `Sidebar`).
- `/pages`: Componentes de maior nível que representam as páginas e rotas da aplicação (ex: `Home`, `DashBoard`).
- `/router`: Arquivos de roteamento (`Router.tsx`) e middlewares de controle de acesso (`ProtectedLayout.tsx`, `RoleMiddleware.tsx`).
- `/assets` e `/styles`: Arquivos estáticos e estilos globais (SCSS).

### Padrões do Mobile (`/app`)
- Aplicativo Android nativo padrão configurado através do `build.gradle.kts`. Entregando consistência com as demais camadas, adota a **Arquitetura Modular Baseada em Features** utilizando o padrão **MVVM com Clean Architecture**. O código central está consolidado no pacote `br.edu.fatecpg`:
  - `/core`: Utilitários centrais como rede e comunicação, contendo instâncias abstratas em cliente (`network/ApiClient.kt` ou `http/ApiClient.kt`), interceptadores de autenticação (`AuthInterceptor.kt`) e mapeamento de dependência local (`TokenManager.kt`).
  - `/feature`: Funcionalidades isoladas (ex: `auth`, `home`, `monitoring`, `realtime`). Cada feature geralmente é estruturada com:
    - `dto/`: Data Classes para serialização via `Gson` e mapeamento da comunicação de entrada/saída com a API.
    - `services/`: Interfaces atreladas ao `Retrofit` e executadas utilizando `suspend functions` para integração com blocos assíncronos.
    - `repository/`: Camada da lógica de dados onde os Serviços são acionados sobre as threads secundárias (`Dispatchers.IO`), retornando encapsuladores seguros de sucesso/falha.
    - `viewmodel/`: Lógica de transição UI gerenciada através de Kotlin Coroutines e emissões via `StateFlow` e `Sealed Classes` (gerindo dinâmicas como `Idle`, `Loading`, `Success` e `Error`).
    - `ui/`: (Ou Telas/Views) Contém os `Fragments`, `Activities` e Telas (Screens) em Jetpack Compose referentes à funcionalidade. Em caso de reaproveitamento, subdiretórios como `components/` também são válidos para isolar `Custom Views` ou `Composables`.
    - `client/`: (Como em features de tempo real) Instâncias de acesso ativo e persistente, como um `WebSocketClient`.

### Padrões do Hardware (`/hardware/esp_bueiro`)
- Código embarcado (C++) em arquivo `.ino`. Concentra lógicas de leitura de sensores e formatação de payload `JSON` no loop de execução para enviar para a rota de medições da API.

---

## 🧑‍💻 Regras para o Assistente (GitHub Copilot)

Quando solicitarem código para este projeto, você deve SEMPRE seguir estas regras restritas:

1. **Separação de Preocupações:** Nunca coloque regras de negócio ou chamadas de banco de dados nos `controllers` (backend) ou diretamente nos componentes de UI (frontend). 
   - Backend: O `Controller` chama o `Service`, que por sua vez chama o `Repository`.
   - Frontend: O `Component` / `Page` usa um `Hook` (`useMinhaFeature`), que chama um `Service` que se apoia no `ApiClient`.
2. **Tipagem e TypeScript:** 
   - No Frontend, crie interfaces detalhadas em `types/index.ts` dentro da feature correspondente.
   - Evite o uso da tipagem `any`. Use `unknown` ou generics quando necessário.
3. **Pydantic (Backend):**
   - Use sempre modelos Pydantic (`dtos.py`) para validar requests e estruturar os responses nas rotas do FastAPI.
4. **Tratamento de Dados Assíncronos:** 
   - No Backend, priorize funções e bibliotecas `async` (como o `httpx`).
   - Mantenha o loop de eventos não-bloqueante.
5. **Estilização (Frontend):** 
   - Os estilos devem ser importados de arquivos `.scss`. 
   - Siga a organização base onde componentes menores ficam na pasta `components` e páginas na pasta `pages`.
6. **Injeção de Dependências:** Respeite as interfaces (`interfaces.py`) existentes no backend. Quando mockar algo para testes, crie a dependência injetada ao longo da raiz da funcionalidade.
7. **Hardware C++:** No código do ESP32 (`/hardware`), não sobrecarregue a memória com alocações dinâmicas exageradas; use `StaticJsonDocument` definindo um tamanho limpo (ex: `<200>`) para envios pequenos via HTTP. Lembre-se: O hardware autentica via Query Token na API, não via JWT.
8. **Mobile Android:** O diretório `/app` concentra código nativo. Sempre verifique e respeite as configurações do gradle (`minSdk 24`) e o namespace do pacote `br.edu.fatecpg`.

## 🚀 Comandos Úteis (Para Referência)
- **Backend:** `uvicorn app.main:app --reload` (Para desenvolvimento local a partir de `/backend`).
- **Frontend:** `npm run dev` (A partir do `/frontend`).
- **Mobile Android:** Abra a pasta `/app` no **Android Studio** para gerenciar o Gradle Build ou rode `./gradlew assembleDebug` via terminal.
- **Hardware IoT:** Utilizar a **Arduino IDE** com placas baseadas em ESP32 e bibliotecas como `ArduinoJson` instaladas para compilar e subir código em `/hardware/esp_bueiro`.
