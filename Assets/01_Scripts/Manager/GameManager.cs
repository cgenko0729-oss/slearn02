using UnityEngine;
using System.Collections;

public class GameManager : Singleton<GameManager>
{   
    [Header("Game Settings")]
    public string gameName = "Template 3D Game 2026";
    public int targetFrameRate = 60;
    public bool isPaused = false;

    void Start()
    {
       
    }

    void Update()
    {
        
    }
}