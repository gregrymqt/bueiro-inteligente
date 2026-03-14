from abc import ABC, abstractmethod

class IRowsService(ABC):
    
    @abstractmethod
    async def append_data(self, spreadsheet_id: str, table_id: str, payload: RowsAppendRequest) -> bool:
        """Adiciona novas linhas no final da tabela especificada."""
        pass
        
    @abstractmethod
    async def create_table(self, spreadsheet_id: str, page_id: str, payload: RowsCreateTableRequest) -> RowsCreateTableResponse:
        """Cria uma nova tabela em uma página existente."""
        pass