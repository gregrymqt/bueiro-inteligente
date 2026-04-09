# 📱 Bueiro Inteligente - Aplicativo Mobile (Android)

Este é o aplicativo móvel nativo do ecossistema **Bueiro Inteligente (Smart Drain)**, desenvolvido em Kotlin. A aplicação permite o monitoramento em tempo real do estado de bueiros inteligentes espalhados pela cidade, exibindo alertas, nível de bloqueio, e dados de manutenção de forma rápida e responsiva.

O ponto de entrada real é `MainActivity -> AppContainer -> AppNavigation`, que centraliza a injeção manual de dependências, o roteamento por telas Compose e a configuração de rede.

## 🛠️ Stack Tecnológica e Ferramentas

- **Linguagem:** Kotlin, rodando na JVM (Java 11)
- **UI Toolkit:** [Jetpack Compose](https://developer.android.com/jetpack/compose) com Material Design 3
- **Arquitetura:** MVVM (Model-View-ViewModel) acoplado com princípios de **Clean Architecture**
- **Navegação:** Jetpack Navigation Compose (`BottomNavRoutes`, `NavHost`)
- **Requisições de Rede (HTTP):** Retrofit 2 + OkHttp + Gson (com Interceptors e Authenticators)
- **Assincronismo e Reatividade:** Kotlin Coroutines e `StateFlow`
- **Ciclo de Vida (Lifecycle):** Uso de `collectAsStateWithLifecycle()` para observação segura de estado

---

## 🏗️ Padrões e Arquitetura

Para garantir manutenibilidade e escalabilidade, o código segue uma divisão rigorosa **Baseada em Features (Feature-Driven Development)**, abstendo-se de acoplamento direto nas telas:

1. **Injeção de Dependências Manual (Service Locator):** O projeto utiliza um `AppContainer` (`core/di/`) para gerenciar as instâncias únicas de repositórios, serviços Web e gerenciadores de Token. Nenhuma view deve instanciar dependências complexas.
2. **Separação de Preocupações (SoC):** 
   - O **ViewModel** lida com a lógica de negócio e expõe UI States fechados (`Sealed Classes`).
   - A **Activity / Screens (Compose)** apenas observa os fluxos de forma _Lifecycle-aware_ e repassa eventos de clique.
3. **Gerenciamento de Navegação Externa Abstraído:** Ações diretas do sistema operacional, como acionar Mapas (Google Maps, Waze), são isoladas pela interface `LocationHandler`, garantindo que Composable Views fiquem desacopladas de Android Intents sensíveis.
4. **Tratamento Global de Rede:** Interceptadores de Sessão (`AuthInterceptor`) anexam tokens nas chamadas automaticamente. O `TokenAuthenticator` atua ativamente caçando retornos 401 para resetar a sessão do usuário de forma reativa.

---

## 📂 Estrutura de Diretórios (`src/main/java/br/edu/fatecpg/`)

```text
├── AppNavigation.kt          # Grafo de navegação principal configurado com AppContainer
├── MainActivity.kt           # Ponto de entrada Android
├── core/                     # Fundações técnicas transversais
│   ├── di/                   # AppContainer.kt (Injeção de Dependência)
│   ├── navigation/           # Roteamento avançado, BottomBar e LocationHandler
│   └── network/              # Cliente HTTP, Interceptors, Autenticadores e TokenManager
└── feature/                  # Módulos Funcionais do Aplicativo
    ├── auth/                 # Login e gerenciamento de permissões (modo visitante/logado)
    ├── home/                 # Dashboards e alertas da tela inicial
    ├── monitoring/           # Visualização dos sensores, listas de bueiros e mapas
    ├── profile/              # Perfil do usuário e funções de Logout (limpeza de backstack)
    └── realtime/             # Sincronização via WebSockets
```

A navegação usa `MainBottomBar`, e todas as telas Compose observam `StateFlow` com `collectAsStateWithLifecycle()`.

---

## ✨ Principais Funcionalidades

- 🛡️ **Guest Mode (Visitante):** O aplicativo permite uso parcial sem login. Telas como `Monitoring` e `Home` adaptam-se dinamicamente (ocultando certas abas ou pedindo login via Modal) caso detectem estado de convidado no `TokenManager`.
- 📊 **Monitoramento Remoto:** A feature `Monitoring` consulta e exibe em tempo real as anomalias, bloqueios e estados dos bueiros.
- 🗺️ **Mapeamento:** Usuários conseguem traçar rotas externas via Google Maps/Waze para ver a localização precisa da tampa de um bueiro através do `LocationHandler`.
- 🔌 **Websockets Ativos:** Integração com um serviço dedicado de Real-time e repositórios baseados em WebSockets nativos (`RealtimeWebSocketClient.kt`).

---

## 🚀 Como Compilar e Executar

**Pré-requisitos:** Android Studio (Iguana ou superior recomendado) e SDK 36 configurado.

**Pelo Android Studio:**
1. Abra o diretório raiz `app/` no Android Studio.
2. Sincronize o projeto usando o Gradle (clique no elefante ou "Sync Project with Gradle Files").
3. Conecte um emulador ou dispositivo físico e clique em ▶️ **Run** (`Shift + F10`).

**Por Linha de Comando (no terminal na raiz `bueiro-inteligente/app/`):**

Para gerar o APK de Debug:
```bash
./gradlew assembleDebug
```

Para instalar e rodar diretamente:
```bash
./gradlew installDebug
```

*(No Windows, substitua `./gradlew` por `gradlew.bat`)*

---

## 🧹 Boas Práticas (Guia para Contribuidores)
- **Não** consuma `StateFlow` em telas via `collectAsState()`. Use SEMPRE `collectAsStateWithLifecycle()` para evitar memory leaks caso o app fique em segundo plano.
- **Novas Features:** Crie um pacote novo dentro de `/feature`. Divída em `/ui`, `/viewmodel`, `/repository`, e `/services`. Atualize o `AppContainer.kt` com instâncias limpas do novo escopo.
- **Requisições:** Atualize as DTOs e não trate erro de parsing diretamente no ViewModel, deixe a camada abstrata do Interceptor pegar e logar as falhas base de conexão.