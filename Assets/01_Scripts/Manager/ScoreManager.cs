using UnityEngine;
using System.Collections;

public class ScoreManager : Singleton<ScoreManager>
{   

   public int currentScore = 0;
    public int highScore = 0;

  

    public void AddScore(int points)
    {
        currentScore += points;
        if (currentScore > highScore)
            highScore = currentScore;
        Debug.Log($"[ScoreManager] Score: {currentScore} (High: {highScore})");
    }

    public void ResetScore()
    {
        currentScore = 0;
    }
}