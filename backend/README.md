# Bueiro Inteligente - Backend

Este é o backend do projeto **Bueiro Inteligente**, responsável por fornecer a API e gerenciar as regras de negócio, persistência de dados e integrações com o IoT.

## Tecnologias Principais

- **[FastAPI](https://fastapi.tiangolo.com/)**: Framework web moderno e rápido para construção de APIs com Python 3.7+ baseado em standard Python type hints.
- **[Uvicorn](https://www.uvicorn.org/)**: Servidor ASGI leve e rápido.
- **[Supabase](https://supabase.com/)**: Alternativa Open Source ao Firebase, utilizado como banco de dados principal.
- **[Redis](https://redis.io/)**: Banco de dados em memória, utilizado para cache, otimização de performance e Blacklist de JWT.
- **Autenticação e Segurança (JWT & RBAC)**: Autenticação baseada em tokens (via `python-jose`), controle de acesso por papéis e criptografia de senhas com `passlib`.
- **Background Jobs (APScheduler)**: Execução de rotinas assíncronas em background (como sincronização de planilhas ETL).
- **WebSockets**: Comunicação em tempo real para os painéis de monitoramento (React/Kotlin).
- **Integrações Externas (`httpx`)**: 
  - Comunicação com hardware IoT.
  - Sincronização e ETL com a plataforma de planilhas de dados **Rows.com**.

## Estrutura de Diretórios

O projeto segue uma arquitetura baseada em features/módulos para facilitar a manutenção e escalabilidade:

```
backend/
├── app/
│   ├── main.py                 # Ponto de entrada da aplicação FastAPI e ciclo de vida
│   ├── core/                   # Configs, DB, cache, segurança (JWT/RBAC), websockets e scheduler
│   └── features/               # Módulos específicos da aplicação (domínios)
│       ├── auth/               # Autenticação de usuários, login e logout (blacklist com Redis)
│       ├── cache/              # Serviços e interfaces para manipulação do Redis
│       ├── monitoring/         # Lógica IoT de bueiros, regras de negócio e rotas de WebSocket
│       └── rows/               # Classes e rotinas de ETL para sincronização de dados via Rows API
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
   pip install -r requirements.txt
   ```

4. **Configuração de Variáveis de Ambiente:**
   - Crie um arquivo `.env` na raiz da pasta `backend/` com base nas configurações esperadas (ex: credenciais do Supabase, URL do Redis, tokens da Adafruit).

5. **Inicie o servidor de desenvolvimento:**
   ```bash
   uvicorn app.main:app --reload
   ```
   A API estará acessível em: `http://localhost:8000`
   
   A documentação interativa (Swagger UI) estará diponível em: `http://localhost:8000/docs`

## Deploy

O projeto inclui um `Dockerfile` que facilita o deploy em ambientes de produção que suportam containers.
