using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameMode {
    HumanVsHuman,
    HumanVsAI,
    AIVsAI
}

public enum Difficulty {
    Easy,
    Medium,
    Hard
}

public static class GameModeController
{
    public static GameMode   gameMode;
    public static string     winner;
    public static string     huPlayer;
    public static string     aiPlayer;
    public static Difficulty difficulty;

    public static bool IsEasy()
    {
        return difficulty == Difficulty.Easy;
    }

    public static bool IsMedium()
    {
        return difficulty == Difficulty.Medium;
    }

    public static bool IsHard()
    {
        return difficulty == Difficulty.Hard;
    }

    public static bool IsHumanVsHuman()
    {
        return gameMode == GameMode.HumanVsHuman;
    }

    public static bool IsHumanVsAI()
    {
        return gameMode == GameMode.HumanVsAI;
    }

    public static bool IsAIVsAI()
    {
        return gameMode == GameMode.AIVsAI;
    }

    public static bool IsPlayer1AI()
    {
        return aiPlayer == "Player1";
    }

    public static bool IsPlayer2AI()
    {
        return aiPlayer == "Player2";
    }
}
