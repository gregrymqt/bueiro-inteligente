# Backend

Backend do projeto Bueiro Inteligente, implementado em ASP.NET Core 8 com Controllers, Razor Pages, SignalR, Entity Framework Core, PostgreSQL e Redis.

## Visao geral

Este backend centraliza as regras de negocio, a persistencia de dados e as integracoes do ecossistema:

- autenticao e autorizacao via JWT
- monitoramento de bueiros em tempo real com SignalR
- leitura e persistencia de dados no PostgreSQL
- cache com Redis
- rotinas agendadas com Quartz
- integracao com Rows para sincronizacao de dados

O projeto usa uma arquitetura por Features para manter controllers, services, repositories e DTOs separados por responsabilidade.

## Tecnologias

- .NET 8
- ASP.NET Core Web App
- Entity Framework Core 8
- Npgsql para PostgreSQL
- StackExchange.Redis
- Quartz
- SignalR
- Razor Pages

## Estrutura do projeto

```text
backend/
├── Program.cs
├── backend.csproj
├── backend.sln
├── Core/
│   └── Settings/
├── extensions/
├── Features/
│   ├── Auth/
│   ├── Home/
│   ├── Monitoring/
│   ├── Realtime/
│   ├── Rows/
│   ├── Drain/
│   └── Drains/
├── Infrastructure/
│   ├── Cache/
│   ├── Extensions/
│   └── Persistence/
├── Pages/
├── Properties/
├── wwwroot/
├── Dockerfile
└── entrypoint.sh
```

## Configuracao de ambiente

O backend carrega o arquivo `.env` e as variaveis de ambiente por meio de `Infrastructure/Extensions/ConfigurationServiceExtensions.cs`, que mapeia os valores para options tipadas em `Core/Settings/` e para `ConnectionStrings`.

Se voce abrir a pasta `backend/` isoladamente, o `.env` do repositorio fica em `../.env`.

As chaves abaixo continuam aceitas tanto no formato plano quanto no formato com `__` usado por Docker e pelo ASP.NET Core.

### Variaveis usadas pelo backend

As variaveis abaixo sao mapeadas para `GeneralSettings`, `JwtSettings`, `GoogleSettings`, `IotSettings`, `RowsSettings` e `ConnectionStrings`:

| Variavel | Descricao |
| --- | --- |
| `PROJECT_NAME` | Nome exibido da aplicacao |
| `VERSION` | Versao do backend |
| `API_STR` | Prefixo base das rotas da API |
| `SECRET_KEY` | Chave usada para JWT |
| `ALGORITHM` | Algoritmo JWT |
| `ACCESS_TOKEN_EXPIRE_MINUTES` | Tempo de expiracao do token |
| `HARDWARE_TOKEN` | Token usado pelo firmware/IoT |
| `ConnectionStrings__DefaultConnection` | String de conexao usada pelo runtime |
| `ConnectionStrings__MigrationsConnection` | String usada no bootstrap de migracoes |
| `ConnectionStrings__Redis` | String de conexao do Redis |
| `ROWS_API_KEY` | Chave de integracao com Rows |
| `ROWS_BASE_URL` | Base URL da API do Rows |
| `ROWS_SPREADSHEET_ID` | ID da planilha |
| `ROWS_TABLE_ID` | ID da tabela |
| `SUPABASE__URL` | URL do projeto Supabase (ex: https://seu-projeto.supabase.co) |
| `SUPABASE__KEY` | Chave de acesso público (anon key) do Supabase |
| `USE__SUPABASE__STORAGE` | Ativa uso de Supabase Storage como destino principal (true/false) |
| `GOOGLE_CLIENT_ID` | Client ID do Google OAuth |
| `GOOGLE_CLIENT_SECRET` | Client Secret do Google OAuth |
| `ALLOWED_HOSTS` | Hosts permitidos pelo backend |
| `EMAIL_USERS_ADMIN` | E-mail do administrador para notificações críticas |
| `ALLOWED_ORIGINS` | Origens permitidas no CORS |
| `APP_ID_SECRET` | Segredo do header `X-App-Id` usado pelo app oficial |

### Exemplo de `.env`

```env
PROJECT_NAME=Bueiro Inteligente
VERSION=1.0.0
API_STR=/api/v1
SECRET_KEY=troque-esta-chave
ALGORITHM=HS256
ACCESS_TOKEN_EXPIRE_MINUTES=30
HARDWARE_TOKEN=token-do-esp32
ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=bueiro_db;Username=bueiro_user;Password=bueiro_password;
ConnectionStrings__MigrationsConnection=Host=localhost;Port=5432;Database=bueiro_db;Username=bueiro_user;Password=bueiro_password;
ConnectionStrings__Redis=localhost:6379,abortConnect=false
ROWS_API_KEY=
ROWS_BASE_URL=https://api.rows.com/v1
ROWS_SPREADSHEET_ID=
ROWS_TABLE_ID=
SUPABASE__URL=
SUPABASE__KEY=
USE__SUPABASE__STORAGE=false
GOOGLE_CLIENT_ID=
GOOGLE_CLIENT_SECRET=
ALLOWED_HOSTS=localhost;127.0.0.1
ALLOWED_ORIGINS=https://localhost:5173,http://localhost:5173
APP_ID_SECRET=
EMAIL_USERS_ADMIN=
```

## Como executar com Docker Compose

Na raiz do repositorio:

```bash
docker compose up -d --build
```

Esse comando sobe:

- backend em `http://localhost:8080`
- PostgreSQL na porta `5432`
- Redis na porta `6379`

O `docker-compose.yml` injeta as variaveis do `.env` da raiz e tambem monta esse arquivo dentro do container em `/app/.env`.

## Como executar localmente

Dentro da pasta `backend/`:

```bash
dotnet restore
```

Para rodar em modo HTTP com o profile do Visual Studio / launchSettings:

```bash
dotnet run --launch-profile http
```

Rotas locais padrao do `launchSettings.json`:

- HTTP: `http://localhost:8080`
- HTTPS: `https://localhost:8081`

## Migracoes do banco

O backend aplica migracoes ao iniciar o servico via `Database.MigrateAsync()`.

No container de desenvolvimento, o `entrypoint.sh` tambem executa:

```bash
dotnet ef database update
```

## Principais rotas

- `/api/v1/auth` - autenticacao e cadastro de usuarios
- `/api/v1/home` - conteudo do painel inicial
- `/api/v1/monitoring` - recebimento e consulta de medições
- `/api/v1/drains` - gerenciamento de bueiros
- `/api/v1/homeadmin` - endpoints administrativos da Home
- `/api/v1/[controller]` - padrao base aplicado aos controllers da API
- `/realtime/ws` - hub SignalR de tempo real

## Testes

A suite de testes fica no projeto `Tests/backend.Tests.csproj`.

Executar a partir da raiz do repositorio:

```bash
dotnet test Tests/backend.Tests.csproj
```

## Observacoes de arquitetura

- controllers chamam services
- services concentram regras de negocio
- repositories acessam o banco de dados
- DTOs validam entradas e saidas
- `ApiControllerBase` padroniza rotas em `api/v1/[controller]`, resposta JSON, autorizacao base e rate limit global
- `Program.cs` apenas registra dependencias e faz o mapeamento das rotas

## Arquivos importantes

- [Program.cs](Program.cs)
- [Core/Settings](Core/Settings)
- [Infrastructure/Extensions/ConfigurationServiceExtensions.cs](Infrastructure/Extensions/ConfigurationServiceExtensions.cs)
- [extensions/App/AppServiceCollectionExtensions.cs](extensions/App/AppServiceCollectionExtensions.cs)
- [extensions/App/Middleware/AppIdMiddleware.cs](extensions/App/Middleware/AppIdMiddleware.cs)
- [Infrastructure/Extensions/DatabaseServiceCollectionExtensions.cs](Infrastructure/Extensions/DatabaseServiceCollectionExtensions.cs)
- [Infrastructure/Extensions/RedisServiceCollectionExtensions.cs](Infrastructure/Extensions/RedisServiceCollectionExtensions.cs)
- [entrypoint.sh](entrypoint.sh)
- [backend.csproj](backend.csproj)
