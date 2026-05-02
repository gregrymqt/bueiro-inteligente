# 🌐 Bueiro Inteligente - Web Dashboard (Frontend)

Este é o aplicativo web (Dashboard) do ecossistema **Bueiro Inteligente (Smart Drain)**. Desenvolvido com **React e TypeScript**, ele oferece uma interface administrativa para monitoramento em tempo real do estado dos bueiros, visualização de estatísticas, alertas e integração com relatórios analíticos.

## 🛠️ Stack Tecnológica

- **Framework Principais:** [React 19](https://react.dev/) + [Vite](https://vitejs.dev/)
- **Linguagem:** TypeScript
- **Roteamento:** React Router DOM (v7)
- **Estilização:** SCSS / CSS Modules padrão
- **Comunicação HTTP e Realtime:** Clientes centralizados em `src/core/http/ApiClient.ts` e `src/core/socket/SignalRClient.ts`, com autenticação via `AuthInterceptor.tsx` e feedback visual via `src/core/alert/AlertService.ts`.
- **Ferramentas de Qualidade:** ESLint configurado para validação rigorosa de tipagem

---

## 🏗️ Padrões e Arquitetura (Feature-Driven)

O projeto adota uma arquitetura modular voltada ao domínio do negócio (**Feature-Driven**), visando escalabilidade e total separação de responsabilidades. Dessa forma, as lógicas de telas, de dados e serviços não ficam "misturadas".

### 📂 Estrutura de Diretórios (src/)

- **/core:** Utilitários essenciais e transversais do sistema.
  - http/: Configuração de clientes HTTP (ApiClient.ts), manipulação de tokens da sessão (TokenService.ts) e interceptadores (AuthInterceptor.tsx).
   - socket/: Cliente global de SignalR compartilhado entre features.
   - alert/: Feedback visual centralizado com SweetAlert2 (`AlertService.ts`).
- **/feature:** O coração das regras de negócio.
  - **auth:** Gestão de acesso e proteção de rotas.
  - **home:** Seções da Landing Page (Hero, HowItWorks, Skeletons).
  - **plan:** Domínio de planos de serviço e precificação.
  - **feedback:** Sistema de avaliações (CRUD completo para usuários).
  - **drain:** Gestão de inventário e cadastro de bueiros (CRUD).
  - **monitoring:** Telemetria em tempo real e análise de dados (Rows).
  - **/components:** Peças de UI reaproveitáveis.
  - layout/: Elementos estruturais da página (Navbar, Sidebar, Footer, MainLayout).
  - ui/: Blocos de construção genéricos (Cards, Carousel, StatusBadge).
- **/pages:** Componentes de alto nível encarregados de unir componentes menores e features às rotas definitivas (ex: Home, Dashboard, Auth).
- **/router:** Central de roteamento baseada em Router.tsx, com isolamento de acesso feito pelas pastas /middleware (ProtectedLayout.tsx, RoleMiddleware.tsx).
- **/styles:** Estilos globais (.scss).
- **/assets:** Arquivos estáticos (imagens, ícones, etc).

---

## ✨ Principais Funcionalidades

- 🔒 **Roteamento Protegido:** Middlewares validam o token JWT de forma reativa e proíbem visitantes não autenticados de entrarem nas rotas de sistema.
- 📊 **Dashboard Dinâmica:** Navegação por abas via Sidebar expansível, permitindo alternar entre monitoramento e gestão.
- 🧩 **UI Responsiva e Modular:** Todo o grid e a estilização confiam amplamente em SCSS padronizado, onde cada componente porta sua própria classe de estilização em paridade (Component.tsx + Component.scss).
- ⚡ **Abstração com Hooks:** Regras de manipulação assíncrona ficam segregadas em Hooks Customizados. As Views apenas montam os visuais e interagem declarativamente a partir dos retornos dos services.

---

## 🚀 Como Executar o Projeto

**Pré-requisitos:** Node.js (v18+) e NPM (ou Yarn/PNPM) instalados.

1. **Acesse o diretório do frontend via terminal:**
   ```bash
   cd frontend
   ```

2. **Instale as dependências essenciais e as de desenvolvimento:**
   ```bash
   npm install
   ```

3. **Inicie o servidor local gerido pelo Vite:**
   ```bash
   npm run dev
   ```
   > A aplicação estará disponível rapidamente na porta padrão 5173. Acesse no navegador em: http://localhost:5173/

4. **Para gerar o build otimizado de produção:**
   ```bash
   npm run build
   ```

---

## 🧹 Boas Práticas para o Desenvolvimento

- **Tipagem Estrita:** Não utilize o tipo `any`. Declare e exporte os contratos na pasta local de types/ atrelada à Feature.
- **Responsabilidade Visual:** Os painéis visuais (.tsx) **nunca** devem chamar uma Promise HTTP diretamente. Use um Custom Hook na Feature que então orquestra as comunicações pelo Service.
- **Importação de SCSS:** Mantenha os estilos isolados nos arquivos da árvore para evitar sobrescritas e vazamentos de regras CSS globais.
- **Integração com ambiente:** O frontend lê `VITE_ROWS_EMBED_URL`, `VITE_BACKEND_URL`, `VITE_BACKEND_LOCAL`, `VITE_WS_URL`, `VITE_WS_LOCAL` e `VITE_ENABLE_RATE_LIMIT`. A decisão entre mock e API agora é feita localmente por feature nos hooks e componentes que consomem os services, sem depender de uma flag global de data source. Se quiser exibir um embed real do Rows no modo backend, defina `VITE_ROWS_EMBED_URL`. Quando `VITE_BACKEND_URL` aponta para `localhost`, `127.0.0.1` ou para um domínio de túnel como `ngrok`/`grok`, o app classifica a URL automaticamente e deriva o SignalR do mesmo host. No login com Google, o frontend envia a origem atual em `frontend_redirect` para o backend validar e reutilizar no retorno. Se `VITE_BACKEND_URL` não estiver definido, `VITE_BACKEND_LOCAL=true` mantém o fallback para `http://localhost:8000`. `VITE_WS_URL` continua como override opcional para cenários remotos específicos. O rate limit client-side pode ser habilitado com `VITE_ENABLE_RATE_LIMIT=true`.
- **Feedback de UI:** Erros, confirmações e sucesso devem passar pelo `AlertService`, não por `window.alert` ou bibliotecas espalhadas em componentes.
