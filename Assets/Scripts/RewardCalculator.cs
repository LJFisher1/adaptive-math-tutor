using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RewardCalculator
{
    public enum Difficulty // This is for the static tutor
    {
        Easy = 0,
        Medium = 1,
        Hard = 2
    }

    public static float ComputeReward(
        bool isCorrect,
        float difficulty,
        int hintCount)
    {
        float reward;

        if (isCorrect)
        {
            reward = 0.5f + (difficulty / 2f);
        }
        else
        {
            reward =- (0.3f + (difficulty / 4f));
        }

        // Hint penalty
        reward -= 0.2f * hintCount;

        return reward;
    }

}

