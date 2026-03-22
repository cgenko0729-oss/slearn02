using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SettingsMenu : UIMenuBase
{   
    [SerializeField] private Slider _volumeSlider;
    [SerializeField] private Toggle _fullscreenToggle;

    private bool _hasUnsavedChanges;

    protected override void OnBeforeOpen()
    {
        // Load current settings into UI
        _volumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
        _fullscreenToggle.isOn = Screen.fullScreen;
        _hasUnsavedChanges = false;
    }

    protected override void OnAfterOpen()
    {
        // Listen for changes
        _volumeSlider.onValueChanged.AddListener(_ => _hasUnsavedChanges = true);
        _fullscreenToggle.onValueChanged.AddListener(_ => _hasUnsavedChanges = true);
    }

    protected override void OnBeforeClose()
    {
        // Auto-save on close
        if (_hasUnsavedChanges)
        {
            PlayerPrefs.SetFloat("MasterVolume", _volumeSlider.value);
            Screen.fullScreen = _fullscreenToggle.isOn;
            PlayerPrefs.Save();
        }
 
        // Unsubscribe
        _volumeSlider.onValueChanged.RemoveAllListeners();
        _fullscreenToggle.onValueChanged.RemoveAllListeners();
    }
 
    public override void OnBackPressed()
    {
        // Could show a "discard changes?" confirmation here instead.
        // For now, just save and close:
        base.OnBackPressed(); // Calls UIMenuManager.Pop()
    }

}