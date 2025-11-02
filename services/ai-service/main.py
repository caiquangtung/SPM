"""
AI Service - FastAPI application
Placeholder for future implementation
"""
from fastapi import FastAPI

app = FastAPI(title="SPM AI Service", version="1.0.0")


@app.get("/health")
async def health_check():
    return {"status": "healthy", "service": "ai-service"}


@app.get("/")
async def root():
    return {"message": "SPM AI Service - Under Construction"}

