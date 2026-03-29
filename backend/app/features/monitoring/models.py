from sqlalchemy import Column, String, Float, DateTime, Boolean, Integer
from datetime import datetime, timezone
from app.core.database import Base

class DrainStatus(Base):
    __tablename__ = "drain_status"

    id = Column(Integer, primary_key=True, index=True, autoincrement=True)
    id_bueiro = Column(String(50), index=True, nullable=False)
    distancia_cm = Column(Float, nullable=False)
    nivel_obstrucao = Column(Float, nullable=False)
    status = Column(String(50), nullable=False)
    latitude = Column(Float, nullable=True)
    longitude = Column(Float, nullable=True)
    ultima_atualizacao = Column(DateTime(timezone=True), default=lambda: datetime.now(timezone.utc))
    sincronizado_rows = Column(Boolean, default=False)
