using UnityEngine;
using System.Collections;

public class GameManager : Singleton<GameManager>
{   
    [Header("Game Settings")]
    public string gameName = "Template 3D Game 2026";
    public int targetFrameRate = 60;
    public bool isPaused = false;

    public PlayerControllerTest playerController;

    public void RegisterPlayer(PlayerControllerTest player)
    {
        playerController = player;
        Debug.Log($"[GameManager] Player registered: {player.gameObject.name}");
    }

    void Start()
    {
       
    }

    void Update()
    {
        
    }
}