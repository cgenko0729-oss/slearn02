using System.Collections.Generic;
using UnityEngine;
 
/// <summary>
/// Stack-based UI menu manager. Handles push/pop navigation, back-button routing,
/// and optional time-scale pausing.
///
/// Design:
///   - Menus are managed as a LIFO stack. Only the topmost menu receives input.
///   - Pushing a new menu notifies the previous top menu via OnLostFocus().
///   - Popping a menu notifies the new top menu via OnRegainedFocus().
///   - Escape/Back input is routed to the topmost menu's OnBackPressed().
///
/// Setup:
///   Place this on a persistent GameObject in your UI scene (or DontDestroyOnLoad).
///   Menus register themselves when pushed — no manual registration needed.
/// </summary>
public class UIMenuManager : Singleton<UIMenuManager>
{   

    [Header("Input")]
    [Tooltip("KeyCode that triggers 'Back' on the top menu. Set to None to disable.")]
    [SerializeField] private KeyCode _backKey = KeyCode.Escape;
 
    [Header("Debug")]
    [SerializeField] private bool _debugLog = false;

    // =========================================================================
    // STATE
    // =========================================================================
 
    private readonly Stack<UIMenuBase> _menuStack = new Stack<UIMenuBase>();
 
    /// <summary>Number of menus currently on the stack.</summary>
    public int Count => _menuStack.Count;
 
    /// <summary>Whether any menu is currently open.</summary>
    public bool HasOpenMenu => _menuStack.Count > 0;
 
    /// <summary>The currently focused (topmost) menu, or null.</summary>
    public UIMenuBase Peek() => _menuStack.Count > 0 ? _menuStack.Peek() : null;
 
    // Track the time scale before any menu paused it, so we can restore it.
    private float _savedTimeScale = 1f;
    private int _pauseRequestCount = 0;

    private void Update()
    {
        // Route back/escape input to the topmost menu
        if (_backKey != KeyCode.None && Input.GetKeyDown(_backKey))
        {
            if (_menuStack.Count > 0)
            {
                UIMenuBase top = _menuStack.Peek();
                if (top.CanReceiveInput)
                {
                    Log($"Back pressed → routing to {top.name}");
                    top.OnBackPressed();
                }
            }
        }
    }
 
    // =========================================================================
    // STACK OPERATIONS
    // =========================================================================
 
    /// <summary>
    /// Opens a menu and pushes it onto the stack.
    /// The previously focused menu receives OnLostFocus().
    /// </summary>
    public void Push(UIMenuBase menu)
    {
        if (menu == null)
        {
            Debug.LogError("[UIMenuManager] Cannot push null menu.");
            return;
        }
 
        if (_menuStack.Contains(menu))
        {
            Debug.LogWarning($"[UIMenuManager] Menu '{menu.name}' is already in the stack. Ignoring Push.");
            return;
        }
 
        // Notify current top that it's losing focus
        if (_menuStack.Count > 0)
        {
            _menuStack.Peek().OnLostFocus();
        }
 
        _menuStack.Push(menu);
        Log($"Pushed '{menu.name}' (stack depth: {_menuStack.Count})");
 
        // Handle pause
        if (menu.PauseGameWhileOpen)
        {
            RequestPause();
        }
 
        menu.Open();
    }
 
    /// <summary>
    /// Closes the topmost menu and pops it from the stack.
    /// The menu below (if any) receives OnRegainedFocus().
    /// </summary>
    public void Pop()
    {
        if (_menuStack.Count == 0)
        {
            Debug.LogWarning("[UIMenuManager] Cannot pop from empty stack.");
            return;
        }
 
        UIMenuBase menu = _menuStack.Pop();
        Log($"Popped '{menu.name}' (stack depth: {_menuStack.Count})");
 
        menu.Close();
 
        // Handle unpause
        if (menu.PauseGameWhileOpen)
        {
            ReleasePause();
        }
 
        // Notify new top that it regained focus
        if (_menuStack.Count > 0)
        {
            _menuStack.Peek().OnRegainedFocus();
        }
    }
 
    /// <summary>
    /// Closes all menus from top to bottom. Useful for "return to gameplay" 
    /// or scene transitions.
    /// </summary>
    public void PopAll()
    {
        Log($"PopAll — clearing {_menuStack.Count} menus");
        while (_menuStack.Count > 0)
        {
            UIMenuBase menu = _menuStack.Pop();
            menu.CloseImmediate(); // Immediate — no staggered animations
 
            if (menu.PauseGameWhileOpen)
            {
                ReleasePause();
            }
        }
    }
 
    /// <summary>
    /// Removes a specific menu from anywhere in the stack (e.g., if it was destroyed).
    /// Prefer Pop() for normal navigation — this is a safety net.
    /// </summary>
    public void Remove(UIMenuBase menu)
    {
        if (!_menuStack.Contains(menu)) return;
 
        // Rebuild the stack without the target menu
        var temp = new Stack<UIMenuBase>();
        while (_menuStack.Count > 0)
        {
            UIMenuBase m = _menuStack.Pop();
            if (m != menu)
                temp.Push(m);
        }
        // Reverse back into _menuStack
        while (temp.Count > 0)
        {
            _menuStack.Push(temp.Pop());
        }
 
        if (menu.PauseGameWhileOpen)
        {
            ReleasePause();
        }
 
        Log($"Removed '{menu.name}' from stack (stack depth: {_menuStack.Count})");
    }
 
    // =========================================================================
    // TIME SCALE MANAGEMENT
    // =========================================================================
 
    private void RequestPause()
    {
        if (_pauseRequestCount == 0)
        {
            _savedTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            Log("Game paused (timeScale = 0)");
        }
        _pauseRequestCount++;
    }
 
    private void ReleasePause()
    {
        _pauseRequestCount = Mathf.Max(0, _pauseRequestCount - 1);
        if (_pauseRequestCount == 0)
        {
            Time.timeScale = _savedTimeScale;
            Log($"Game unpaused (timeScale = {_savedTimeScale})");
        }
    }

    // =========================================================================
    // DEBUG
    // =========================================================================
 
    private void Log(string message)
    {
        if (_debugLog)
            Debug.Log($"[UIMenuManager] {message}");
    }
}