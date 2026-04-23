using UnityEngine;
using static RewardCalculator;

public static class RLProblemSelector
{
    public static Difficulty SelectNextDifficulty(
        Difficulty currentDifficulty,
        float reward
    )
    {
        int next = (int)currentDifficulty;

        if (reward > 0f)
            next++;          // reward -> harder
        else if (reward < 0f)
            next--;          // penalty -> easier
        // reward == 0 -> same difficulty

        // Safety clamp
        next = Mathf.Clamp(next, (int)Difficulty.Easy, (int)Difficulty.Hard);

        //Debug.Log(
        //    $"[RL] Difficulty: {currentDifficulty} -> {(Difficulty)next} (reward={reward})"
        //);

        return (Difficulty)next;
    }
}
