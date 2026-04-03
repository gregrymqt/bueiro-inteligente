from app.core.database import Base
from app.features.auth.models import User, Role
from app.features.monitoring.models import DrainStatus
from app.features.home.models import  CarouselModel, StatCardModel

# Este arquivo centraliza o import da classe Base e de todos os models da aplicaÃ§Ã£o.
# O Alembic usarÃ¡ este arquivo para ler o target_metadata corretamente e rastrear todas as tabelas.