# Bueiro Inteligente - Backend

Este é o backend do projeto **Bueiro Inteligente**, responsável por fornecer a API e gerenciar as regras de negócio, persistência de dados e integrações com o IoT.

## Tecnologias Principais

- **[FastAPI](https://fastapi.tiangolo.com/)**: Framework web moderno e rápido para construção de APIs com Python 3.10+ baseado em standard Python type hints.
- **[Uvicorn](https://www.uvicorn.org/)**: Servidor ASGI leve e rápido.
- **[PostgreSQL](https://www.postgresql.org/)**: Sistema gerenciador de banco de dados relacional (utilizado com **SQLAlchemy** e **Alembic** para migrações).
- **[Redis](https://redis.io/)**: Banco de dados em memória, utilizado para cache, otimização de performance e Blacklist de JWT.
- **Autenticação e Segurança (JWT & RBAC)**: Autenticação baseada em tokens (via `python-jose`), controle de acesso por papéis e criptografia de senhas com `passlib`.
- **Hardware IoT**: A rota `POST /monitoring/medicoes` recebe o token do ESP32 via query string `?token=...` e também aceita Bearer token para cenários compatíveis.
- **Background Jobs (APScheduler)**: Execução de rotinas assíncronas em background (como sincronização de planilhas ETL).
- **WebSockets**: Comunicação em tempo real para os painéis de monitoramento (React/Kotlin).
- **Integrações Externas (`httpx`)**: 
  - Comunicação com hardware IoT.
  - Sincronização e ETL com a plataforma de planilhas de dados **Rows.com**.
- **Testes**: `pytest`, `pytest-asyncio`, `pytest-mock` e `aiosqlite` para a suíte em `backend/tests/`.

## Estrutura de Diretórios

O projeto segue uma arquitetura baseada em features/módulos para facilitar a manutenção e escalabilidade:

```text
backend/
├── alembic/                    # Arquivos e versões de migração do banco (Alembic)
├── app/
│   ├── main.py                 # Ponto de entrada da aplicação FastAPI e ciclo de vida
│   ├── core/                   # Configurações globais, banco de dados (database.py) e variáveis
│   ├── extensions/             # Configurações de infraestrutura e serviços core (auth, infrastructure, realtime, scheduler)
│   ├── routes/                 # Registro centralizado de rotas (agregação de controllers)
│   └── features/               # Módulos específicos de negócio (features)
│       ├── auth/               # Autenticação de usuários, roles e gestão de JWT
│       ├── cache/              # Serviços centralizados para manipulação do Redis
│       ├── home/               # Lógica para o Dashboard inicial
│       ├── monitoring/         # Lógica IoT de bueiros, alertas, validações e status
│       ├── realtime/           # Gerenciamento de WebSockets
│       └── rows/               # Componentes de Job/Services de planilhas ETL integradas
├── alembic.ini                 # Configuração do Alembic
├── entrypoint.sh               # Script de execução em containers (roda as migrações no boot)
├── Dockerfile                  # Instruções para conteinerização da aplicação
└── requirements.txt            # Dependências em Python do projeto
```

## Configuração do Ambiente de Desenvolvimento

### Pré-requisitos

- Python 3.10+ (recomendado)
- Docker e Docker Compose (opcional, para execução isolada)

### Passos para rodar localmente

1. **Clone o repositório e acesse a pasta do backend:**
   ```bash
   cd backend
   ```

2. **Crie e ative um ambiente virtual:**
   ```bash
   python -m venv venv
   # No Windows:
   venv\Scripts\activate
   # No Linux/macOS:
   source venv/bin/activate
   ```

3. **Instale as dependências:**
   ```bash
   pip install --upgrade pip
   pip install -r requirements.txt
   ```

4. **Configuração de Variáveis de Ambiente:**
   - Crie um arquivo `.env` na raiz do diretório `backend` e preencha as chaves de banco, Redis, JWT, Rows e hardware token.
   - As variáveis mais importantes são `SECRET_KEY`, `ALGORITHM`, `ACCESS_TOKEN_EXPIRE_MINUTES`, `DATABASE_URL_LOCAL`, `DATABASE_URL_CLOUD`, `DB_LOCAL`, `REDIS_URL`, `REDIS_LOCAL`, `HARDWARE_TOKEN`, `ROWS_API_KEY`, `ROWS_SPREADSHEET_ID` e `ROWS_TABLE_ID`.
   - O backend lê automaticamente esse arquivo via `pydantic-settings`.

5. **Execute as Migrações do Banco:**
   ```bash
   alembic upgrade head
   ```

6. **Inicie o servidor de desenvolvimento:**
   ```bash
   uvicorn app.main:app --reload
   ```
   A API estará acessível em: `http://localhost:8000`
   
   A documentação interativa (Swagger UI) estará disponível em: `http://localhost:8000/docs`

## Testes

Execute a suíte com:

```bash
pytest tests/ -v
```

As fixtures principais ficam em `backend/tests/conftest.py`, com overrides de dependência e banco SQLite em memória para testes isolados.

## Deploy

O projeto inclui um `Dockerfile` que facilita o deploy em ambientes de produção que suportam containers.
