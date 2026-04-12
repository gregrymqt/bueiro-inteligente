## 🛠 Stack Tecnológica
### Backend (C# / .NET 8)
- **Framework Principal:** ASP.NET Core 8 com C#.
- **Interface Web:** Controllers, Razor Pages e SignalR.
- **Persistência:** PostgreSQL com Entity Framework Core e Npgsql.
- **Cache:** Redis com StackExchange.Redis.
- **Jobs/Workers:** Quartz para tarefas agendadas.
- **Autenticação:** JWT com extensões e serviços próprios do backend.
- **Validação de Dados:** DTOs C# fortes, nullable reference types e Data Annotations quando necessário.
- **Estrutura/Padrões:**
   - Arquitetura baseada em Features (`backend/Features/`).
   - Configuração central em `backend/core/AppSettings.cs`, com leitura automática de `.env`.
   - Injeção de dependências via `backend/extensions/` e `backend/Infrastructure/Extensions/`.
   - Padrão `Controller -> Service -> Repository` para isolar regras de negócio e acesso a dados.
   - `Program.cs` atua como composition root, registrando serviços, middlewares, hubs SignalR e rotas.

### Testes do Backend (`/Tests`)
- **Projeto de Testes:** `Tests/backend.Tests.csproj`.
- **Stack de Testes:** `xUnit`, `Moq`, `FluentAssertions` e `coverlet.collector`.
- **Estrutura:** namespaces `backend.Tests.Features.<Feature>` e arquivos organizados por feature.
- **Boas Práticas:** AAA, mocks estritos quando fizer sentido, `VerifyNoOtherCalls()` em cenários sensíveis e isolamento de dependências externas.
- **Banco de Dados:** prefira mocks, fakes ou fixtures ao banco real sempre que existir uma alternativa equivalente e confiável.
## 🧭 Arquitetura e Integração

O ecossistema é centrado no backend ASP.NET Core. Cada frente conversa com ele por contratos específicos:

### Padrões do Backend (`/backend`)
- `/core`: Configurações globais e carregamento do `.env` (`AppSettings.cs`).
- `/extensions`: Extensões de registro e inicialização de serviços da aplicação.
- `/Infrastructure/Extensions`: Bootstrap de banco, Redis e demais integrações de infraestrutura.
- `/Infrastructure/Persistence`: `AppDbContext`, `UnitOfWork` e contratos de persistência.
- `/Infrastructure/Cache`: abstrações e implementações de cache Redis.
- `/Features`: Módulos de negócio isolados. Cada feature pode conter:
   - `Presentation/`: Controllers, hubs e endpoints.
   - `Application/`: Services, DTOs e contratos.
   - `Domain/`: Entidades, interfaces e regras centrais.
   - `Infrastructure/`: Repositories e implementações de acesso a dados.
- `/Pages`: Razor Pages do backend.
- `/Program.cs`: Composição da aplicação, registro de dependências e mapeamento de hubs/rotas.
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
3. **DTOs e Validação (Backend C#):**
   - Use sempre DTOs fortes (`record`, `class` ou `struct` quando apropriado) para requests e responses.
   - Prefira validação explícita, nullable reference types e Data Annotations quando fizer sentido.
   - Mantenha controllers leves e deixe regra de negócio em services.
4. **Tratamento de Dados Assíncronos:** 
   - No Backend, priorize funções e bibliotecas `async` (como o `httpx`).
   - Mantenha o loop de eventos não-bloqueante.
5. **Estilização (Frontend):** 
   - Os estilos devem ser importados de arquivos `.scss`. 
   - Siga a organização base onde componentes menores ficam na pasta `components` e páginas na pasta `pages`.
6. **Injeção de Dependências:** Respeite os contratos e interfaces existentes no backend. Quando mockar algo para testes, crie a dependência injetada ao longo da raiz da funcionalidade.
7. **Hardware C++:** No código do ESP32 (`/hardware`), não sobrecarregue a memória com alocações dinâmicas exageradas; use `StaticJsonDocument` definindo um tamanho limpo (ex: `<200>`) para envios pequenos via HTTP. Lembre-se: O hardware autentica via Query Token na API, não via JWT.
8. **Mobile Android:** O diretório `/app` concentra código nativo. Sempre verifique e respeite as configurações do gradle (`minSdk 24`) e o namespace do pacote `br.edu.fatecpg`.
9. **Mobile Compose e Estado:** Em telas Compose, consuma `StateFlow` exclusivamente com `collectAsStateWithLifecycle()`. Mantenha navegação, intents e abertura de mapas fora dos Composables; use a abstração `LocationHandler` e o fluxo `MainActivity -> AppContainer -> AppNavigation`.
10. **Testes do Backend:** Quando criar lógica complexa em services, repositories ou integrações com Rows, adicione testes em `Tests/Features/<feature>/`. Use `xUnit`, `Moq`, `FluentAssertions`, mocks de dependência e testes async quando necessário; nunca acople teste ao banco real quando existir fixture equivalente.
11. **Hardware e Credenciais:** Ao sugerir alterações em `esp_bueiro.ino`, mantenha credenciais em `secrets.h` e preserve a autenticação por query token `?token=` na API. Evite JWT no firmware e prefira `StaticJsonDocument` para payloads pequenos.
12. **Frontend HTTP e Alertas:** Nunca use `fetch`, `axios` ou chamadas HTTP diretas em componentes e páginas. Todo acesso à API deve passar por `src/core/http/ApiClient.ts` e pelos services da feature; feedback visual deve usar `src/core/alert/AlertService.ts`, e não `window.alert`.
## 🚀 Comandos Úteis (Para Referência)
- **Backend:** `dotnet run --project backend/backend.csproj` (a partir da raiz) ou `dotnet run` dentro de `/backend`.

## 🚀 Comandos Úteis (Para Referência)
- **Backend:** `uvicorn app.main:app --reload` (Para desenvolvimento local a partir de `/backend`).
- **Testes do Backend:** `dotnet test Tests/backend.Tests.csproj`.
- **Frontend:** `npm run dev` (A partir do `/frontend`).
- **Mobile Android:** Abra a pasta `/app` no **Android Studio** para gerenciar o Gradle Build ou rode `./gradlew assembleDebug` via terminal.
- **Hardware IoT:** Utilizar a **Arduino IDE** com placas baseadas em ESP32 e bibliotecas como `ArduinoJson` instaladas para compilar e subir código em `/hardware/esp_bueiro`.
