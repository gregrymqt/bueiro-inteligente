import pytest
from app.core.config import Settings

def test_settings_db_local_true():
    # Test when DB_LOCAL is True, DATABASE_URL should be DATABASE_URL_LOCAL
    settings = Settings(DB_LOCAL=True, DATABASE_URL_LOCAL="postgresql://local", DATABASE_URL_CLOUD="postgresql://cloud")
    assert settings.DATABASE_URL == "postgresql://local"

def test_settings_db_local_false():
    # Test when DB_LOCAL is False, DATABASE_URL should be DATABASE_URL_CLOUD
    settings = Settings(DB_LOCAL=False, DATABASE_URL_LOCAL="postgresql://local", DATABASE_URL_CLOUD="postgresql://cloud")
    assert settings.DATABASE_URL == "postgresql://cloud"

def test_settings_redis_local_flag():
    # Test basic default of REDIS_LOCAL
    settings = Settings(REDIS_LOCAL=True)
    assert settings.REDIS_LOCAL is True

    settings_false = Settings(REDIS_LOCAL=False)
    assert settings_false.REDIS_LOCAL is False
