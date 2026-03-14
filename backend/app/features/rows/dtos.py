from pydantic import BaseModel, Field

# ==========================================
# DTOs para Criação de Tabela (Ref: Foto 2)
# ==========================================
class RowsCreateTableRequest(BaseModel):
    name: str = Field(..., description="Nome da nova tabela dentro da página do Rows")

class RowsCreateTableResponse(BaseModel):
    id: str
    name: str
    slug: str
    created_at: str

# ==========================================
# DTOs para Overwrite (Ref: Foto 3)
# Ideal para definir os Cabeçalhos (Headers)
# ==========================================
class CellData(BaseModel):
    value: str | int | float | bool | None = None
    formula: str | None = None

class RowsOverwriteRequest(BaseModel):
    # Uma lista de listas. A lista de fora são as linhas, a de dentro são as colunas.
    cells: list[list[CellData]] = Field(
        ..., 
        description="Matriz contendo objetos de células (com value ou formula)."
    )

# ==========================================
# DTOs para Append (Ref: Foto 4)
# O coração do nosso ETL de dados do bueiro
# ==========================================
class RowsAppendRequest(BaseModel):
    # O Append é mais simples, aceita os valores diretos sem precisar da key "value:"
    values: list[list[str | int | float | bool]] = Field(
        ..., 
        description="Matriz de valores simples para serem adicionados na próxima linha vazia."
    )