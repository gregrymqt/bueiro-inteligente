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
├── core/
│   └── AppSettings.cs
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

O backend carrega um arquivo `.env` automaticamente pelo `AppSettings` em `core/AppSettings.cs`. O arquivo e procurado a partir de `AppContext.BaseDirectory`, subindo na arvore de diretorios ate encontrar o `.env`.

Se voce abrir a pasta `backend/` isoladamente, o `.env` do repositorio fica em `../.env`.

### Variaveis usadas pelo backend

As variaveis abaixo sao lidas pelo `AppSettings`:

| Variavel | Descricao |
| --- | --- |
| `PROJECT_NAME` | Nome exibido da aplicacao |
| `VERSION` | Versao do backend |
| `API_STR` | Prefixo base das rotas da API |
| `SECRET_KEY` | Chave usada para JWT |
| `ALGORITHM` | Algoritmo JWT |
| `ACCESS_TOKEN_EXPIRE_MINUTES` | Tempo de expiracao do token |
| `HARDWARE_TOKEN` | Token usado pelo firmware/IoT |
| `REDIS_URL` | URL do Redis quando `REDIS_LOCAL=false` |
| `REDIS_LOCAL` | Alterna entre Redis local e remoto |
| `DB_LOCAL` | Alterna entre banco local e cloud |
| `DATABASE_URL_CLOUD` | URL do banco em cloud |
| `DATABASE_URL_LOCAL` | URL do banco local |
| `MIGRATIONS_URL` | URL usada para migracoes |
| `ROWS_API_KEY` | Chave de integracao com Rows |
| `ROWS_BASE_URL` | Base URL da API do Rows |
| `ROWS_SPREADSHEET_ID` | ID da planilha |
| `ROWS_TABLE_ID` | ID da tabela |
| `ALLOWED_ORIGINS` | Origens permitidas no CORS |

### Exemplo de `.env`

```env
PROJECT_NAME=Bueiro Inteligente
VERSION=1.0.0
API_STR=/api/v1
SECRET_KEY=troque-esta-chave
ALGORITHM=HS256
ACCESS_TOKEN_EXPIRE_MINUTES=30
HARDWARE_TOKEN=token-do-esp32
REDIS_URL=redis://redis:6379/0
REDIS_LOCAL=true
DB_LOCAL=true
DATABASE_URL_CLOUD=
DATABASE_URL_LOCAL=postgresql+asyncpg://bueiro_user:bueiro_password@db:5432/bueiro_db
MIGRATIONS_URL=postgresql+psycopg2://bueiro_user:bueiro_password@db:5432/bueiro_db
ROWS_API_KEY=
ROWS_BASE_URL=https://api.rows.com/v1
ROWS_SPREADSHEET_ID=
ROWS_TABLE_ID=
ALLOWED_ORIGINS=https://localhost:5173,http://localhost:5173
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

- HTTP: `http://localhost:5273`
- HTTPS: `https://localhost:7061`

## Migracoes do banco

O backend aplica migracoes ao iniciar o servico via `Database.MigrateAsync()`.

No container de desenvolvimento, o `entrypoint.sh` tambem executa:

```bash
dotnet ef database update
```

## Principais rotas

- `/auth` - autenticacao e cadastro de usuarios
- `/home` - conteudo do painel inicial
- `/monitoring` - recebimento e consulta de medições
- `/drains` - gerenciamento de bueiros
- `/rows` - integracao e sincronizacao com Rows
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
- `Program.cs` apenas registra dependencias e faz o mapeamento das rotas

## Arquivos importantes

- [Program.cs](Program.cs)
- [core/AppSettings.cs](core/AppSettings.cs)
- [Infrastructure/Extensions/DatabaseServiceCollectionExtensions.cs](Infrastructure/Extensions/DatabaseServiceCollectionExtensions.cs)
- [Infrastructure/Extensions/RedisServiceCollectionExtensions.cs](Infrastructure/Extensions/RedisServiceCollectionExtensions.cs)
- [entrypoint.sh](entrypoint.sh)
- [backend.csproj](backend.csproj)
