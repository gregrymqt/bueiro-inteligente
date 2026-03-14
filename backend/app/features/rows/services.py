import logging
from interfaces import IRowsService
import httpx
from fastapi import HTTPException

# Importando os DTOs que criamos no passo anterior
from .dtos import RowsAppendRequest, RowsCreateTableRequest, RowsCreateTableResponse

# Configuração básica de log para a feature
logger = logging.getLogger(__name__)

class RowsService(IRowsService):
    def __init__(self, api_key: str):
        self.api_key = api_key
        self.base_url = "https://api.rows.com/v1"
        self.headers = {
            "Authorization": f"Bearer {self.api_key}",
            "Content-Type": "application/json",
            "Accept": "application/json"
        }

    async def append_data(
            self,
            spreadsheet_id: str,
            table_id: str,
            payload: RowsAppendRequest) -> bool:
        # Endpoint padrão do Rows para Append
        endpoint = f"{self.base_url}/spreadsheets/{spreadsheet_id}/tables/{table_id}/values:append"
        
        # httpx.AsyncClient gerencia a sessão assíncrona perfeitamente
        async with httpx.AsyncClient() as client:
            try:
                logger.info(f"Iniciando envio de lote para a tabela {table_id}...")
                
                # dump_model() converte o Pydantic DTO para um dicionário Python seguro para JSON
                response = await client.post(
                    endpoint, 
                    headers=self.headers, 
                    json=payload.model_dump() 
                )
                
                # Lança uma exceção se o status HTTP for de erro (4xx ou 5xx)
                response.raise_for_status()
                
                logger.info(f"Lote de dados inserido com sucesso na tabela {table_id}.")
                return True
                
            except httpx.HTTPStatusError as exc:
                # Erros que a própria API do Rows retornou (Ex: 401 Unauthorized, 400 Bad Request)
                logger.error(f"Erro na API do Rows: {exc.response.status_code} - {exc.response.text}")
                # Repassando como HTTPException do FastAPI para o Controller lidar adequadamente
                raise HTTPException(status_code=exc.response.status_code, detail="Erro ao integrar com a plataforma Rows.")
                
            except httpx.RequestError as exc:
                # Erros de rede, timeout, DNS (O servidor do Rows caiu ou estamos sem internet)
                logger.error(f"Falha de conexão ao tentar acessar o Rows: {exc}")
                raise HTTPException(status_code=503, detail="Serviço de visualização temporariamente indisponível.")
            
            except Exception as exc:
                # Catch-all para erros inesperados
                logger.critical(f"Erro inesperado no RowsService: {str(exc)}")
                raise HTTPException(status_code=500, detail="Erro interno no servidor durante o ETL.")

    async def create_table(
            self,
            spreadsheet_id: str,
            page_id: str,
            payload: RowsCreateTableRequest) -> RowsCreateTableResponse:
        endpoint = f"{self.base_url}/spreadsheets/{spreadsheet_id}/pages/{page_id}/tables"
        
        async with httpx.AsyncClient() as client:
            try:
                response = await client.post(
                    endpoint, 
                    headers=self.headers, 
                    json=payload.model_dump()
                )
                response.raise_for_status()
                
                # Retorna validando a resposta diretamente no DTO de Response
                return RowsCreateTableResponse(**response.json())
                
            except httpx.HTTPError as exc:
                logger.error(f"Falha ao criar tabela: {exc}")
                raise HTTPException(status_code=500, detail="Falha ao criar infraestrutura no Rows.")