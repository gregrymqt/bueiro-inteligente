# Bueiro Inteligente - Backend

Este é o backend do projeto **Bueiro Inteligente**, responsável por fornecer a API e gerenciar as regras de negócio, persistência de dados e integrações com o IoT.

## Tecnologias Principais

- **[FastAPI](https://fastapi.tiangolo.com/)**: Framework web moderno e rápido para construção de APIs com Python 3.7+ baseado em standard Python type hints.
- **[Uvicorn](https://www.uvicorn.org/)**: Servidor ASGI leve e rápido.
- **[PostgreSQL](https://www.postgresql.org/)**: Sistema gerenciador de banco de dados relacional (utilizado com **SQLAlchemy** e **Alembic** para migrações).
- **[Redis](https://redis.io/)**: Banco de dados em memória, utilizado para cache, otimização de performance e Blacklist de JWT.
- **Autenticação e Segurança (JWT & RBAC)**: Autenticação baseada em tokens (via `python-jose`), controle de acesso por papéis e criptografia de senhas com `passlib`.
- **Background Jobs (APScheduler)**: Execução de rotinas assíncronas em background (como sincronização de planilhas ETL).
- **WebSockets**: Comunicação em tempo real para os painéis de monitoramento (React/Kotlin).
- **Integrações Externas (`httpx`)**: 
  - Comunicação com hardware IoT.
  - Sincronização e ETL com a plataforma de planilhas de dados **Rows.com**.

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
   - Crie um arquivo `.env` na raiz do diretório `backend`, não esquecendo das configurações principais de Banco (PostgreSQL - Supabase), Instância Redis e os tokens e segredos (como da placa e Rows.com). A porta local padrão do Uvicorn se configurará aos scripts de *run*.
   - A configuração já suporta fallbacks para *Redis* local, e para ambientes de *Deploy* em Nuvem ele utilizará as flags passadas (`REDIS_LOCAL=False`).

5. **Execute as Migrações do Banco:**
   ```bash
   alembic upgrade head
   ```

6. **Inicie o servidor de desenvolvimento:**
   ```bash
   uvicorn app.main:app --reload
   ```
   A API estará acessível em: `http://localhost:8000`
   
   A documentação interativa (Swagger UI) estará diponível em: `http://localhost:8000/docs`

## Deploy

O projeto inclui um `Dockerfile` que facilita o deploy em ambientes de produção que suportam containers.
