from fastapi import FastAPI
from pydantic import BaseModel
import sqlite3

app = FastAPI()

# Define the expected telemetry structure
class Telemetry(BaseModel):
    sessionId: str
    tutorMode: str
    problemId: str
    difficulty: float
    isCorrect: bool
    responseTimeSec: float
    hintCount: int
    timestamp: str
    educationLevel: str
    lastMathClass: str


# Create database + table if not exists
def init_db():
    conn = sqlite3.connect("telemetry.db")
    cursor = conn.cursor()
    cursor.execute("""
        CREATE TABLE IF NOT EXISTS telemetry (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            sessionId TEXT,
            tutorMode TEXT,
            problemId TEXT,
            difficulty FLOAT,
            isCorrect INTEGER,
            responseTimeSec REAL,
            hintCount INTEGER,
            timestamp TEXT,
            educationLevel TEXT,
            lastMathClass TEXT
        )
    """)
    conn.commit()
    conn.close()

init_db()


@app.post("/telemetry")
def receive_telemetry(data: Telemetry):
    conn = sqlite3.connect("telemetry.db")
    cursor = conn.cursor()
    cursor.execute("""
        INSERT INTO telemetry (
            sessionId, tutorMode, problemId,
            difficulty, isCorrect,
            responseTimeSec, hintCount, timestamp,
            educationLevel, lastMathClass
        )
        VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
    """, (
        data.sessionId,
        data.tutorMode,
        data.problemId,
        data.difficulty,
        int(data.isCorrect),
        data.responseTimeSec,
        data.hintCount,
        data.timestamp,
        data.educationLevel,
        data.lastMathClass
    ))
    conn.commit()
    conn.close()

    return {"status": "ok"}

@app.get("/telemetry")
def get_telemetry():
    conn = sqlite3.connect("telemetry.db")
    cursor = conn.cursor()
    cursor.execute("SELECT * FROM telemetry")
    rows = cursor.fetchall()
    conn.close()
    return rows
