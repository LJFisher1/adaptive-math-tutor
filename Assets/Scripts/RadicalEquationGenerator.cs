using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;



public class RadicalEquation
{
    public string EquationText;
    public int A; // Inside the radical
    public int B; // right-hand side
    public List<int> ValidSolutions;
    public bool IsCorrect(int userAnswer)
    {
        return ValidSolutions.Contains(userAnswer);
    }
}
public class RadicalEquationGenerator : MonoBehaviour
{
    public RadicalEquation Generate(int a, int b)
    {
        // Build equation text
        string equation = $"sqrt(x + {a}) = x - {b}";

        RadicalEquation eq = new RadicalEquation();
        eq.EquationText = equation;
        eq.A = a;
        eq.B = b;
        eq.ValidSolutions = new List<int>();

        // Solve: sqrt(x + a) = x - b

        // x^2 - (2b + 1)x + (b^2 - a) = 0

        int A = 1;
        int B = -(2 * b + 1);
        int C = (b * b) - a;

        int discriminant = B * B - 4 * A * C;

        if (discriminant >= 0)
        {

            int sqrtD = (int)Mathf.Sqrt(discriminant);

            if (sqrtD * sqrtD == discriminant)
            {
                int denom = 2 * A;

                int num1 = -B + sqrtD;
                if (num1 % denom == 0)
                {
                    int x = num1 / denom;
                    if (IsValidSolution(x, a, b))
                    {
                        eq.ValidSolutions.Add(x);
                    }
                }

                int num2 = -B - sqrtD;
                if (num2 % denom == 0)
                {
                    int x = num2 / denom;
                    if (!eq.ValidSolutions.Contains(x) && IsValidSolution(x, a, b))
                    {
                        eq.ValidSolutions.Add(x);
                    }
                }
            }
        }

        if (eq.ValidSolutions.Count == 0)
        {
            eq.ValidSolutions.Add(-1);
        }

        return eq;
    }

    private bool IsValidSolution(int x, int a, int b)
    {
        if (x - b < 0) return false;
        if (x + a < 0) return false;

        return (x + a) == (x - b) * (x - b);
    }


}
