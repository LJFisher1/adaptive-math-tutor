using System;

[Serializable]
public class TelemetryRecord
{
    public string sessionId;
    public int questionIndex;
    public string tutorMode;
    public string problemId;
    public float difficulty;
    public bool isCorrect;
    public float responseTimeSec;
    public int hintCount;
    public string timestamp;

    // Intake Data
    public string educationLevel;
    public string lastMathClass;
}
