# Bueiro Inteligente - Backend

Este é o backend do projeto **Bueiro Inteligente**, responsável por fornecer a API e gerenciar as regras de negócio, persistência de dados e integrações com o IoT.

## Tecnologias Principais

- **[FastAPI](https://fastapi.tiangolo.com/)**: Framework web moderno e rápido para construção de APIs com Python 3.7+ baseado em standard Python type hints.
- **[Uvicorn](https://www.uvicorn.org/)**: Servidor ASGI leve e rápido.
- **[Supabase](https://supabase.com/)**: Alternativa Open Source ao Firebase, utilizado como banco de dados principal.
- **[Redis](https://redis.io/)**: Banco de dados em memória, utilizado para cache e otimização de performance.
- **Integração IoT**: Comunicação com a API da **Adafruit** via `httpx` para coleta de dados dos sensores.

## Estrutura de Diretórios

O projeto segue uma arquitetura baseada em features/módulos para facilitar a manutenção e escalabilidade:

```
backend/
├── app/
│   ├── main.py                 # Ponto de entrada da aplicação FastAPI
│   ├── core/                   # Configurações globais, banco de dados, segurança e cache
│   └── features/               # Módulos específicos da aplicação (domínios)
│       ├── cache/              # Serviços e interfaces para manipulação do Redis
│       └── monitoring/         # Lógica de negócio, controllers e integração para monitoramento
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
