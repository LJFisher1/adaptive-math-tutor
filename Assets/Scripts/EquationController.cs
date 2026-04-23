using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;
using static RewardCalculator;

public class EquationController : MonoBehaviour
{
    //public enum SessionState
    //{
    //    NotStarted,
    //    QuestionActive,
    //    AnswerSubmitted,
    //    SessionEnded
    //}
    private enum TutorMode
    {
        Baseline,
        Adaptive
    }

    private TutorMode currentMode;


    [Header("UI")]
    [SerializeField] private GameObject tutorPanel;
    [SerializeField] private GameObject intakePanel;
    [SerializeField] private TMP_Text equationTextUI;
    [SerializeField] private TMP_InputField answerInputX;
    [SerializeField] private TMP_InputField answerInputY;
    //[SerializeField] private Toggle noSolutionToggle;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private GameObject resultsPanel;
    [SerializeField] private TMP_Text resultsSummaryText;
    [SerializeField] private Button hintButton;
    [SerializeField] private TMP_Text sessionInfoText;

    [Header("Logic")]
    //[SerializeField] private RadicalEquationGenerator generator;
    [SerializeField] private SimultaneousEquationGenerator sEquationGenerator;
    [SerializeField] private bool useBaselineSequence;

    [Header("Tracking")]
    [SerializeField] private bool developmentMode = true;
    private Difficulty currentDifficulty = Difficulty.Medium;
    private float adaptiveDifficulty = 1f;
    private string currentSessionId;
    private float problemStartTime;
    private int hintCount;
    private string educationLevel;
    private string lastMathClass;

    //[SerializeField] private bool runGenerationTest = false;
    //private int genTotal = 0;
    //private int genSolvable = 0;
    //private int genNoSolution = 0;

    private int baselineIndex = 0; // 0-2 Easy, 3-5 Medium, 6-8 Hard
    private const int QUESTIONS_PER_MODE = 9;
    private const int QUESTIONS_PER_SESSION = QUESTIONS_PER_MODE * 2;
    private int questionsAnswered = 0;
    private int questionIndex = 0;
    private int correctAnswers = 0;
    private int currentHintIndex = 0;

    // Radicals
    private readonly (int a, int b)[] baselinePairs =
    {
        // Easy
        (-1,3),     // 5
        (2,2),      // No Solution
        (0,2),      // 4
        (6,0),      // 3
        (0,0),      // 0

        // Medium
        (16, 4),    // 9
        (2, 5),     // No solution
        (41, 1),    // 8
        (-3, 5),    // 7
        (55, 1),    // 9

        // Hard
        (3, 9),     // 13
        (35, 7),    // 14
        (12, 12),   // 13, 12
        (88, 2),    // 12
        (14, 7)     // No solution
    };

    // Simultaneous -- These are placeholders currently
    private readonly (int a1, int b1, int c1, int a2, int b2, int c2)[] baselineSystems =
    {
        // Easy
        (5,7,9, 3,7,11),        // x = -1, y = 2
        (8,-3, 30, 5,-3,21),    // x = 3, y = -2
        (3,-2,2, 5,-6,-10),     // x = 4, y = 5

        // Medium
        (2,1,7, 7,2,20),        // x = 2, y = 3
        (1,2,7, -2,5,4),        // x = 3, y = 2
        (5,-1,19, 13,-3,51),    // x = 3, y = -4

        // Hard
        (-1,3,1, 4,-5,3),       // x = 2, y = 1
        (6,5,-4, 3,-4,11),      // x = 1, y = -2
        (4,1,9, -3,2,-4)        // x = 2, y = 1

    };

    private readonly Difficulty[] baselineDifficulties =
    {
        // Easy (3)
        Difficulty.Easy,
        Difficulty.Easy,
        Difficulty.Easy,

        // Medium (3)
        Difficulty.Medium,
        Difficulty.Medium,
        Difficulty.Medium,

        // Hard (3)
        Difficulty.Hard,
        Difficulty.Hard,
        Difficulty.Hard,

    };

    private readonly string[] radicalHints =
    {
        "1. Square both sides of the equation to eliminate the square root.",
        "2. Expand the squared expression on the right side.",
        "3. Move all terms to one side to form a quadratic equation.",
        "4. Solve the quadratic and check each solution in the original equation."
    };

    private readonly string[] simultaneousHints =
    {
        "1. Pick one equation and solve it for either x or y.",
        "2. Rewrite that equation so one variable is alone (for example: x = ...).",
        "3. Substitute that expression into the other equation and solve for the remaining variable.",
        "4. Plug that value back into one of the original equations to find the second variable."
    };


    //private RadicalEquation currentEquation;
    private SimultaneousEquation sCurrentEquation;


    void Start()
    {

        //if (runGenerationTest)
        //{
        //    RunGenerationTest(1000);
        //    return;
        //}

        resultsPanel.SetActive(false);
        tutorPanel.SetActive(false);

        if (intakePanel != null)
            intakePanel.SetActive(true);

    }

    public void BeginSession(string education, string lastMath)
    {
        educationLevel = education;
        lastMathClass = lastMath;

       // Debug.Log($"INTAKE STORED: {educationLevel} | {lastMathClass}");

        tutorPanel.SetActive(true);

        currentMode = TutorMode.Baseline;


        // Make sure hint button is off for baseline
        hintButton.gameObject.SetActive(false);

        currentSessionId = System.Guid.NewGuid().ToString();

        questionsAnswered = 0;
        questionIndex = 0;
        correctAnswers = 0;
        currentHintIndex = 0;
        hintCount = 0;
        baselineIndex = 0;
        adaptiveDifficulty = 1f;

        ClearFeedback();
        answerInputX.text = "";
        answerInputY.text = "";

        GenerateNewEquation();
    }

    // Radical Adaptive Parameters
    //private (int a, int b) GetParametersForAdaptiveDifficulty(float difficulty)
    //{
    //    // Range for X
    //    int xMin, xMax;

    //    if (difficulty < 0.66f) // Easy
    //    {
    //        xMin = 1;
    //        xMax = 5;
    //    }

    //    else if (difficulty < 1.33f) // Medium
    //    {
    //        xMin = 6;
    //        xMax = 10;
    //    }

    //    else // Hard
    //    {
    //        xMin = 11;
    //        xMax = 16;
    //    }

    //    // Generate a solvable base equation
    //    int x = Random.Range(xMin, xMax + 1);
    //    int b = Random.Range(0, x);
    //    int a = (x - b) * (x - b) - x;

    //    // 80/20 mix: 80% keep solvable, 20% break it into no-solution
    //    if (Random.value >= 0.8f)
    //    {
    //        a += Random.Range(1, 3); // small change that usually removes the solution
    //    }

    //    return (a, b);
    //}

    private (int a1, int b1, int c1, int a2, int b2, int c2)
    GetSystemParametersForAdaptiveDifficulty(float difficulty)
    {
        int xyMin, xyMax;
        int coefMin, coefMax;

        if (difficulty < 0.66f)
        {
            xyMin = 1;
            xyMax = 5;
            coefMin = -5;
            coefMax = 5;
        }
        else if (difficulty < 1.33f)
        {
            xyMin = -5;
            xyMax = 8;
            coefMin = -8;
            coefMax = 8;
        }
        else
        {
            xyMin = -10;
            xyMax = 12;
            coefMin = -12;
            coefMax = 12;
        }

        int x = Random.Range(xyMin, xyMax + 1);
        int y = Random.Range(xyMin, xyMax + 1);

        int a1 = RandomNonZero(coefMin, coefMax);
        int b1 = RandomNonZero(coefMin, coefMax);


        int a2, b2;

        // Easy
        if (difficulty < 0.66f)
        {
            a2 = RandomNonZero(coefMin, coefMax);
            b2 = b1;
        }
        // Medium
        else if (difficulty < 1.33f)
        {
            int multiplier = Random.Range(2, 4);
            a2 = a1 * multiplier;
            b2 = RandomNonZero(coefMin, coefMax);
        }
        // Hard
        else
        {
            a2 = RandomNonZero(coefMin, coefMax);
            b2 = RandomNonZero(coefMin, coefMax);
        }

        // Prevent parallel equations
        while (a1 * b2 - a2 * b1 == 0)
        {
            a2 = Random.Range(coefMin, coefMax + 1);
        }

        int c1 = a1 * x + b1 * y;
        int c2 = a2 * x + b2 * y;

        return (a1, b1, c1, a2, b2, c2);
    }
    private int RandomNonZero(int min, int max)
    {
        int value = Random.Range(min, max + 1);

        while (value == 0)
        {
            value = Random.Range(min, max + 1);
        }

        return value;
    }

    void GenerateNewEquation()
    {
        //int a;
        //int b;

        switch (currentMode)
        {
            case TutorMode.Baseline:

                if (baselineIndex >= baselineSystems.Length)
                {
                    currentMode = TutorMode.Adaptive;
                    goto case TutorMode.Adaptive;
                }

                currentDifficulty = baselineDifficulties[baselineIndex];

                // Radicals
                //var pair = baselinePairs[baselineIndex];
                //baselineIndex++;

                //a = pair.a;
                //b = pair.b;
                var sys = baselineSystems[baselineIndex];
                baselineIndex++;

                sCurrentEquation = sEquationGenerator.Generate(
                    sys.a1, sys.b1, sys.c1,
                    sys.a2, sys.b2, sys.c2
                    );
                break;

            case TutorMode.Adaptive:
                // Radicals
                //var adaptivePair = GetParametersForAdaptiveDifficulty(adaptiveDifficulty);
                //a = adaptivePair.a;
                //b = adaptivePair.b;
                //// Debug.Log($"[RL] Generating {currentDifficulty} equation with a={a}, b={b}");
                ///

                var adaptiveSys = GetSystemParametersForAdaptiveDifficulty(adaptiveDifficulty);

                sCurrentEquation = sEquationGenerator.Generate(
                    adaptiveSys.a1,
                    adaptiveSys.b1,
                    adaptiveSys.c1,
                    adaptiveSys.a2,
                    adaptiveSys.b2,
                    adaptiveSys.c2
                );

                break;

            default:
                // Radicals
                //    a = 4;
                //    b = 2;
                //    break;
                var fallback = baselineSystems[0];

                sCurrentEquation = sEquationGenerator.Generate(
                    fallback.a1, fallback.b1, fallback.c1,
                    fallback.a2, fallback.b2, fallback.c2
                );
                break;
        }

        // Hint button only active in adaptive mode
        hintButton.gameObject.SetActive(currentMode == TutorMode.Adaptive);

        UpdateProblemDisplay();
        sessionInfoText.text = $"Current Mode: {currentMode}\nQuestion: {questionsAnswered + 1}/{QUESTIONS_PER_SESSION}";
        problemStartTime = Time.time;

        // Reset hint indices
        currentHintIndex = 0;
        hintCount = 0;
        hintButton.interactable = true;

        // Debug.Log("Equation used by UI: " + currentEquation.EquationText);
        // Debug.Log(
        //    currentEquation.ValidSolutions.Count == 0
        //        ? "Expected Solutions: NONE"
        //        : "Expected Solutions: " + string.Join(", ", currentEquation.ValidSolutions)
        //);
    }


    public void SubmitAnswer()
    {
        if (!tutorPanel.activeSelf)
            return;

        ClearFeedback();
        // Radicals
        //Debug.Log($"CHECKING: {sCurrentEquation.EquationText}  SOL={string.Join(",", sCurrentEquation.ValidSolutions)}");
        // Simultaneous
        //Debug.Log($"CHECKING: {sCurrentEquation.EquationText}  SOL=({sCurrentEquation.SolutionX},{sCurrentEquation.SolutionY})");

        //List<int> userAnswers = new List<int>();

        //if (noSolutionToggle.isOn)
        //{
        //    userAnswers.Add(-1);
        //}
        //else
        //{
        //    if (int.TryParse(answerInput1.text, out int a1))
        //        userAnswers.Add(a1);

        //    if (int.TryParse(answerInput2.text, out int a2))
        //        userAnswers.Add(a2);
        //}
        int userX;
        int userY;

        bool parsedX = int.TryParse(answerInputX.text, out userX);
        bool parsedY = int.TryParse(answerInputY.text, out userY);

        bool correct = false;

        if (parsedX && parsedY)
        {
            correct = sCurrentEquation.IsCorrect(userX, userY);
        }

        // Were the answers correct? - Radicals
        //bool correct = AreAnswersCorrect(userAnswers, sCurrentEquation.ValidSolutions);
        // Debug.Log($"User submitted {userAnswer}: {(correct ? "CORRECT" : "INCORRECT")}");

        // How long did it take in MS?
        float responseTimeSec = (Time.time - problemStartTime);
        // Debug.Log($"User took {responseTimeMs}ms to answer");

        // Build the record of all metrics for this question
        TelemetryRecord record = BuildTelemetryRecord(correct, responseTimeSec);
        //Debug.Log(JsonUtility.ToJson(record));

        StartCoroutine(SendTelemetry(record));

        questionIndex++;
        questionsAnswered++;

        if (correct)
        {
            correctAnswers++;
        }

        ShowFeedback(correct);

        // Swaps to adaptive mode if question amount met
        if (questionsAnswered == QUESTIONS_PER_MODE)
        {
            currentMode = TutorMode.Adaptive;
            //Debug.Log("Swapping to adaptive tutor mode");
        }

        // RL hook (adaptive mode only)
        if (currentMode == TutorMode.Adaptive)
        {
            float reward = RewardCalculator.ComputeReward(
                correct,
                adaptiveDifficulty,
                hintCount);

            adaptiveDifficulty = Mathf.Clamp(
                adaptiveDifficulty + reward * 1f, 0f, 2f
            );

            //Debug.Log($"[RL] Current difficulty now: {adaptiveDifficulty}");
        }

        if (questionsAnswered >= QUESTIONS_PER_SESSION)
        {
            EndSession();
            return;
        }

        // Reset input
        answerInputX.text = "";
        answerInputY.text = "";
        //noSolutionToggle.isOn = false;


        // Advance to next question
        GenerateNewEquation();
    }
    private void UpdateProblemDisplay()
    {
        equationTextUI.text = sCurrentEquation.EquationText;
    }

    private void ShowFeedback(bool isCorrect)
    {
        if (currentMode != TutorMode.Adaptive)
            return;

        feedbackText.text = isCorrect ? "Correct" : "Incorrect";
        feedbackText.gameObject.SetActive(true);
    }

    private void ClearFeedback()
    {
        feedbackText.gameObject.SetActive(false);
        feedbackText.text = "";
    }

    private void EndSession()
    {
        tutorPanel.SetActive(false);
        resultsPanel.SetActive(true);

        resultsSummaryText.text = $"You answered {correctAnswers} out of {questionsAnswered} questions correctly.";

    }

    public void UseHint()
    {
        if (currentHintIndex == 0)
        {
            feedbackText.text = "";
        }

        if (currentHintIndex < simultaneousHints.Length)
        {
            if (!string.IsNullOrEmpty(feedbackText.text))
            {
                feedbackText.text += "\n";
            }
            feedbackText.text += simultaneousHints[currentHintIndex];
            feedbackText.gameObject.SetActive(true);

            currentHintIndex++;
            hintCount++;
            // Debug.Log($"Hint used. Total hints this question: {hintCount}");
        }
        if (currentHintIndex >= simultaneousHints.Length)
        {
            hintButton.interactable = false;
        }

    }

    private TelemetryRecord BuildTelemetryRecord(bool correct, float responseTimeSec)
    {
        return new TelemetryRecord
        {
            sessionId = developmentMode
            ? "DEV-" + currentSessionId
            : currentSessionId,

            questionIndex = questionIndex,

            tutorMode = currentMode.ToString(),
            problemId = sCurrentEquation.EquationText,
            difficulty = currentMode == TutorMode.Adaptive
                ? adaptiveDifficulty
                : (float)currentDifficulty,
            isCorrect = correct,
            responseTimeSec = responseTimeSec,
            hintCount = hintCount,
            timestamp = System.DateTime.UtcNow.ToString("o"),

            // Intake
            educationLevel = educationLevel,
            lastMathClass = lastMathClass
        };
    }

    private IEnumerator SendTelemetry(TelemetryRecord record)
    {
        string json = JsonUtility.ToJson(record);

        using (UnityWebRequest request = new UnityWebRequest(
            "https://adaptive-tutor-telemetry.onrender.com/telemetry",
            "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Telemetry failed: " + request.error);
            }
            else
            {
                Debug.Log("Telemetry sent successfully.");
            }
        }
    }

    // Radicals
    //private void RunGenerationTest(int count)
    //{
    //    string path = Application.dataPath + "/genlog.txt";
    //    File.WriteAllText(path, ""); // clears file at start

    //    int solvable = 0;
    //    int noSolution = 0;

    //    for (int i = 0; i < count; i++)
    //    {
    //        var pair = GetParametersForAdaptiveDifficulty(adaptiveDifficulty);
    //        var eq = generator.Generate(pair.a, pair.b);

    //        bool hasRealSolution =
    //            eq.ValidSolutions != null &&
    //            eq.ValidSolutions.Count > 0 &&
    //            !(eq.ValidSolutions.Count == 1 && eq.ValidSolutions[0] == -1);

    //        File.AppendAllText(
    //            path,
    //            $"{i + 1:000}: {eq.EquationText} " +
    //            (hasRealSolution
    //                ? string.Join(",", eq.ValidSolutions)
    //                : "NO SOLUTION") + "\n"
    //        );


    //        if (hasRealSolution)
    //        {
    //            solvable++;
    //        }
    //        else
    //        {
    //            noSolution++;
    //        }
    //    }

    //    float solvPct = solvable * 100f / count;
    //    float noPct = noSolution * 100f / count;

    //    File.AppendAllText(path, $"\nTOTAL {count} | Solvable={solvable} | NoSol={noSolution}\n");

    //    //Debug.Log($"[GEN TEST] Total={count}  Solvable={solvable} ({solvPct:F1}%)  NoSolution={noSolution} ({noPct:F1}%)");
    //}

    public void OnNoSolutionToggleChanged(bool isOn)
    {

        answerInputX.interactable = !isOn;
        answerInputY.interactable = !isOn;

        if (isOn)
        {
            answerInputX.text = "";
            answerInputY.text = "";

            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(null);
        }
    }

    private bool AreAnswersCorrect(List<int> user, List<int> expected)
    {
        // No solution
        if (expected.Count == 1 && expected[0] == -1)
        {
            return user.Count == 1 && user[0] == -1;
        }

        // Count must match
        if (user.Count != expected.Count)
        {
            return false;
        }

        // Every user answer must exist in expected
        foreach (var ans in user)
        {
            if (!expected.Contains(ans))
            {
                return false;
            }
        }

        return true;
    }

    public void RestartSession()
    {
        resultsPanel.SetActive(false);
        BeginSession(educationLevel, lastMathClass);
    }

    public void ExitProgram()
    {
        //Debug.Log("Quit called");
        Application.Quit();
    }


}
