using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using UnityEngine;

public class SimultaneousEquation
{
    public string EquationText;

    // Equation 1
    public int A1, B1, C1;
    // Equation 2
    public int A2, B2, C2;

    public int SolutionX;
    public int SolutionY;

    public bool IsCorrect(int x, int y)
    {
        return x == SolutionX && y == SolutionY;
    }
}
public class SimultaneousEquationGenerator : MonoBehaviour
{
    public SimultaneousEquation Generate(int a1, int b1, int c1, int a2, int b2, int c2)
    {
        SimultaneousEquation eq = new SimultaneousEquation();

        eq.A1 = a1;
        eq.B1 = b1;
        eq.C1 = c1;

        eq.A2 = a2;
        eq.B2 = b2;
        eq.C2 = c2;

        eq.EquationText = FormatEquation(a1, b1, c1, a2, b2, c2);

        SolveSimultaneous(eq);

        return eq;         
    }
    
    private string FormatEquation(int a1, int b1, int c1, int a2, int b2, int c2)
    {
        string eq1 = $"{a1}x {FormatTerm(b1, "y")} = {c1}";
        string eq2 = $"{a2}x {FormatTerm(b2, "y")} = {c2}";

        return eq1 + "\n" + eq2;
    }

    private string FormatTerm(int coefficient, string variable)
    {
        if (coefficient >= 0)
            return $"+ {coefficient}{variable}";
        else
            return $"- {Mathf.Abs(coefficient)}{variable}";
    }

    private void SolveSimultaneous(SimultaneousEquation eq)
    {
        int a1 = eq.A1;
        int b1 = eq.B1;
        int c1 = eq.C1;

        int a2 = eq.A2;
        int b2 = eq.B2;
        int c2 = eq.C2;

        int det = a1 * b2 - a2 * b1;

        if (det == 0)
        {
            //Debug.LogError("Invalid system: determinant is zero (parallel equations).");
            return;
        }

        int x = (c1 * b2 - c2 * b1) / det;
        int y = (a1 * c2 - a2 * c1) / det;
        //Debug.Log($"Raw values -> a1={a1}, b1={b1}, c1={c1}, a2={a2}, b2={b2}, c2={c2}, det={det}, x={x}, y={y}");

        //Debug.Log($"SOLVING: {a1}x + {b1}y = {c1}");
        //Debug.Log($"         {a2}x + {b2}y = {c2}");
        //Debug.Log($"det = {a1 * b2 - a2 * b1}");
        //Debug.Log($"Computed solution: ({x},{y})");
        eq.SolutionX = x;
        eq.SolutionY = y;
    }
}
