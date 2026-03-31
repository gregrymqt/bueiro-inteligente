import uuid
import enum
from sqlalchemy import Column, String, Integer, Enum
from sqlalchemy.dialects.postgresql import UUID
from app.core.database import Base

class CarouselSectionEnum(str, enum.Enum):
    hero = "hero"
    alerts = "alerts"
    stats = "stats"

class StatCardColorEnum(str, enum.Enum):
    success = "success"
    warning = "warning"
    danger = "danger"

class CarouselModel(Base):
    __tablename__ = "home_carousels"

    id = Column(UUID(as_uuid=True), primary_key=True, default=uuid.uuid4, index=True)
    title = Column(String, nullable=False)
    subtitle = Column(String, nullable=True)
    image_url = Column(String, nullable=False)
    action_url = Column(String, nullable=True)
    order = Column(Integer, nullable=False, default=0)
    section = Column(Enum(CarouselSectionEnum, name="carousel_section_enum"), nullable=False)


class StatCardModel(Base):
    __tablename__ = "home_stats"

    id = Column(UUID(as_uuid=True), primary_key=True, default=uuid.uuid4, index=True)
    title = Column(String, nullable=False)
    value = Column(String, nullable=False)
    description = Column(String, nullable=False)
    icon_name = Column(String, nullable=False)
    color = Column(Enum(StatCardColorEnum, name="statcard_color_enum"), nullable=False)
    order = Column(Integer, nullable=False, default=0)
