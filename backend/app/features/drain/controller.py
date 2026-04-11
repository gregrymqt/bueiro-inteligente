from typing import List
from fastapi import APIRouter, Depends, status

from app.extensions.infrastructure import get_db
from app.extensions.auth import RoleChecker
from sqlalchemy.ext.asyncio import AsyncSession

from .dto import DrainCreate, DrainUpdate, DrainRead
from .repository import DrainRepository
from .service import DrainService

router = APIRouter(prefix="/drains", tags=["Drains"])

def get_drain_service(db: AsyncSession = Depends(get_db)) -> DrainService:
    repository = DrainRepository(db_client=db)
    return DrainService(repository=repository)

@router.get("", response_model=List[DrainRead], dependencies=[Depends(RoleChecker(['User', 'Admin', 'Manager']))])
async def list_drains(
    skip: int = 0,
    limit: int = 100,
    service: DrainService = Depends(get_drain_service)
):
    return await service.get_all_drains(skip=skip, limit=limit)

@router.get("/{drain_id}", response_model=DrainRead, dependencies=[Depends(RoleChecker(['User', 'Admin', 'Manager']))])
async def get_drain(
    drain_id: int,
    service: DrainService = Depends(get_drain_service)
):
    return await service.get_drain_by_id(drain_id)

@router.post("", response_model=DrainRead, status_code=status.HTTP_201_CREATED, dependencies=[Depends(RoleChecker(['Admin', 'Manager']))])
async def create_drain(
    data: DrainCreate,
    service: DrainService = Depends(get_drain_service)
):
    return await service.create_drain(data)

@router.put("/{drain_id}", response_model=DrainRead, dependencies=[Depends(RoleChecker(['Admin', 'Manager']))])
async def update_drain(
    drain_id: int,
    data: DrainUpdate,
    service: DrainService = Depends(get_drain_service)
):
    return await service.update_drain(drain_id, data)

@router.delete("/{drain_id}", status_code=status.HTTP_204_NO_CONTENT, dependencies=[Depends(RoleChecker(['Admin']))])
async def delete_drain(
    drain_id: int,
    service: DrainService = Depends(get_drain_service)
):
    await service.delete_drain(drain_id)
