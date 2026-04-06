using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : UIMenuBase
{   

    [Header("Pause Menu References")]
    [SerializeField] private Button _resumeButton; 
    [SerializeField] private Button _settingsButton;
    [SerializeField] private Button _quitButton;
    [SerializeField] private SettingsMenu _settingsMenu; // Direct reference to child menu

    protected override void Awake()
    {
        base.Awake();
        
        // Listen for button clicks
        if (_resumeButton != null)
            _resumeButton.onClick.AddListener(OnResumeClicked);
            
        if (_settingsButton != null)
            _settingsButton.onClick.AddListener(OnSettingsClicked);
            
        if (_quitButton != null)
            _quitButton.onClick.AddListener(OnQuitClicked);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        // Clean up listeners when destroyed
        if (_resumeButton != null)
            _resumeButton.onClick.RemoveListener(OnResumeClicked);
            
        if (_settingsButton != null)
            _settingsButton.onClick.RemoveListener(OnSettingsClicked);
            
        if (_quitButton != null)
            _quitButton.onClick.RemoveListener(OnQuitClicked);
    }

    private void OnResumeClicked()
    {
        // Pop() removes this menu from the stack and closes it, resuming the game
        UIMenuManager.Instance.Pop();
    }

    private void OnSettingsClicked()
    {
        // Push the settings menu on top of the pause menu
        if (_settingsMenu != null)
        {
            UIMenuManager.Instance.Push(_settingsMenu);
        }
    }

    private void OnQuitClicked()
    {
        // Handle quitting the app or returning to main menu
        Debug.Log("Quit Game...");
        Application.Quit();
    }
}