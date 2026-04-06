using UnityEngine;
using System.Collections;

public class GameManager : Singleton<GameManager>
{   
    [Header("Game Settings")]
    public string gameName = "Template 3D Game 2026";
    public int targetFrameRate = 60;
    public bool isPaused = false;

    public PlayerControllerTest playerController;

    public HealthSystem playerHealth;


    public enum Difficulty { Easy, Normal, Hard, Nightmare }
    [Header("Difficulty")]
    public Difficulty currentDifficulty = Difficulty.Normal;
    public float difficultyMultiplier = 1.0f;
    public enum GameMode { SinglePlayer, CoopStory, PvPArena, Sandbox }
    [Header("Game Mode")]
    public GameMode currentMode = GameMode.SinglePlayer;
    public int maxPlayersForMode = 1;

    [Header("Combat")]
    public WeaponSystem playerWeapon;

    [SerializeField] private PauseMenu pauseMenu;

    public void OnPlayerDeath()
    {
        Debug.Log("[GameManager] Player has died! Showing game over...");
        //PauseGame();
        // TODO: Show game over UI
    }

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
        // For testing: Press P to toggle pause
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (!UIMenuManager.Instance.HasOpenMenu)
            {
                UIMenuManager.Instance.Push(pauseMenu);
            }
        }

    }
}