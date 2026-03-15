# Contexto Geral do Projeto: Bueiro Inteligente (Smart Drain)

Este é um projeto full-stack (Frontend + Backend) que monitoriza o estado de bueiros inteligentes e possivelmente sincroniza dados (ETL). O projeto adota uma arquitetura modular baseada em "Features" (funcionalidades), separando responsabilidades e facilitando a manutenção.

## 🛠 Stack Tecnológica

### Backend (Python)
- **Framework Principal:** FastAPI (com Uvicorn)
- **Linguagem:** Python 3.x
- **Bancos de Dados/Cache:** Supabase e Redis
- **Autenticação:** JWT (python-jose, passlib)
- **Validação de Dados:** Pydantic / Pydantic Settings
- **Agendamento (Jobs/Workers):** APScheduler
- **Comunicações HTTP (Assíncronas):** HTTPX (Usado para integrações, ex: Adafruit, Rows)
- **Estrutura/Padrões:**
  - Arquitetura baseada em Features (`app/features/`).
  - Princípios de Inversão de Dependência (Dependency Injection via interfaces).
  - Padrão Repository e Services para isolar regras de negócio.

### Frontend (React + TypeScript)
- **Framework Principal:** React 19 (criado com Vite)
- **Linguagem:** TypeScript
- **Roteamento:** React Router DOM (v7)
- **Estilização:** SCSS / CSS Modules (Padrão: nomes de arquivos como `Component.scss`)
- **Gestão de Estado/Hooks:** Custom Hooks por feature (`useLogin.ts`, `useDrainStatus.ts`)
- **Estrutura/Padrões:**
  - Arquitetura baseada em Features (`src/feature/`).
  - Serviços de HTTP isolados (`src/core/http/ApiClient.ts`, `TokenService.ts`).
  - Componentização modular (`src/components/`, `src/pages/`).

---

## 📁 Estrutura de Diretórios e Padrões Arquiteturais

### Padrões do Backend (`/backend/app`)
- `/core`: Configurações globais, conexão com banco de dados, cache, schedulers, middleware e segurança.
- `/features`: Módulos de negócio isolados. Cada feature geralmente contém:
  - `controller.py`: Endpoints do FastAPI (Rotas).
  - `service.py`: Regras de negócio da aplicação.
  - `repository.py`: Interação direta com a camada de dados (Supabase/Redis).
  - `interfaces.py`: Classes abstratas / Tipagens para injeção de dependência.
  - `dto.py` (ou `dtos.py`): Modelos Pydantic para validação de entrada/saída (Schemas).

### Padrões do Frontend (`/frontend/src`)
- `/core`: Utilitários centrais como clientes HTTP base e manipulação de tokens.
- `/feature`: Regras de negócio e hooks divididos por contexto (ex: `auth`, `monitoring`).
- `/components`: Componentes reutilizáveis (UI) ou de Layout estrutural (UI e View).
- `/pages`: Componentes de maior nível que representam rotas do sistema.
- `/assets` e `/styles`: Arquivos estáticos e SCSS globais.

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

## 🚀 Comandos Úteis (Para Referência)
- **Backend:** `uvicorn app.main:app --reload` (Para desenvolvimento local a partir de `/backend`).
- **Frontend:** `npm run dev` (A partir do `/frontend`).
