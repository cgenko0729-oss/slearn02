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

    }

}