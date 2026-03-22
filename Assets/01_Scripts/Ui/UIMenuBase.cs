using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
 
/// <summary>
/// Abstract base class for all UI menus in the game.
/// 
/// Design Principles:
///   - Template Method Pattern: Open/Close define the skeleton; subclasses override hooks.
///   - CanvasGroup-based: transitions via alpha/interactable/blocksRaycasts, NOT SetActive.
///   - Stack-aware: integrates with UIMenuManager for push/pop navigation.
///   - Transition-safe: prevents input during animations; exposes IsTransitioning guard.
///
/// Usage:
///   1. Attach to a UI GameObject that has a CanvasGroup component.
///   2. Subclass and override hooks (OnBeforeOpen, OnAfterOpen, OnBackPressed, etc.)
///   3. Optionally override PlayOpenAnimation / PlayCloseAnimation for custom transitions.
///   4. Call UIMenuManager.Instance.Push(myMenu) to open, or myMenu.Open() directly.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public abstract class UIMenuBase : MonoBehaviour
{
    // =========================================================================
    // SERIALIZED FIELDS
    // =========================================================================
 
    [Header("Menu Settings")]
    [Tooltip("If true, pressing Back/Escape will close this menu.")]
    [SerializeField] private bool _allowBackClose = true;
 
    [Tooltip("If true, the menu starts hidden on Awake.")]
    [SerializeField] private bool _hideOnAwake = true;
 
    [Tooltip("If true, pauses the game (Time.timeScale = 0) while this menu is open.")]
    [SerializeField] private bool _pauseGameWhileOpen = false;
 
    [Header("Default Transition Settings")]
    [Tooltip("Duration of the default fade transition in seconds.")]
    [SerializeField] private float _transitionDuration = 0.25f;
 
    [Tooltip("Easing curve for transitions. Defaults to smooth ease-in-out.")]
    [SerializeField] private AnimationCurve _transitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
 
    [Header("Audio (Optional)")]
    [SerializeField] private AudioClip _openSound;
    [SerializeField] private AudioClip _closeSound;
 
    [Header("Events")]
    public UnityEvent OnMenuOpened;
    public UnityEvent OnMenuClosed;
 
    // =========================================================================
    // STATE (Read-Only Properties)
    // =========================================================================
 
    /// <summary>Whether this menu is currently visible and active.</summary>
    public bool IsOpen { get; private set; }
 
    /// <summary>Whether an open/close animation is currently playing.</summary>
    public bool IsTransitioning { get; private set; }
 
    /// <summary>Whether this menu is the topmost in the UIMenuManager stack.</summary>
    public bool IsOnTop => UIMenuManager.Instance != null && UIMenuManager.Instance.Peek() == this;
 
    /// <summary>Whether this menu is ready to receive input (open, not transitioning, on top).</summary>
    public bool CanReceiveInput => IsOpen && !IsTransitioning && IsOnTop;
 
    public bool AllowBackClose => _allowBackClose;
    public bool PauseGameWhileOpen => _pauseGameWhileOpen;
 
    // =========================================================================
    // CACHED REFERENCES
    // =========================================================================
 
    private CanvasGroup _canvasGroup;
    public CanvasGroup CanvasGroup
    {
        get
        {
            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();
            return _canvasGroup;
        }
    }
 
    private RectTransform _rectTransform;
    public RectTransform RectTransform
    {
        get
        {
            if (_rectTransform == null)
                _rectTransform = GetComponent<RectTransform>();
            return _rectTransform;
        }
    }
 
    private Coroutine _activeTransition;
 
    // =========================================================================
    // UNITY LIFECYCLE
    // =========================================================================
 
    protected virtual void Awake()
    {
        // Cache components early
        _canvasGroup = GetComponent<CanvasGroup>();
        _rectTransform = GetComponent<RectTransform>();
 
        if (_hideOnAwake)
        {
            SetCanvasGroupState(0f, false, false);
            IsOpen = false;
        }
    }
 
    protected virtual void OnDestroy()
    {
        // Clean up: remove from stack if we're destroyed while open
        if (IsOpen && UIMenuManager.Instance != null)
        {
            UIMenuManager.Instance.Remove(this);
        }
    }
 
    /// <summary>
    /// Called every frame ONLY when this menu is open and on top of the stack.
    /// Subclasses override this instead of Update() to avoid unnecessary polling.
    /// </summary>
    protected virtual void OnUpdate() { }
 
    private void Update()
    {
        if (!CanReceiveInput) return;
        OnUpdate();
    }
 
    // =========================================================================
    // OPEN / CLOSE — Template Method Pattern
    // =========================================================================
 
    /// <summary>
    /// Opens this menu. Follows the lifecycle:
    ///   OnBeforeOpen() → Animation → OnAfterOpen() → Event
    /// Safe to call multiple times (no-ops if already open or transitioning).
    /// </summary>
    public void Open()
    {
        if (IsOpen || IsTransitioning) return;
 
        // Cancel any lingering transition
        StopActiveTransition();
 
        // Lifecycle hook — subclass can set up data, refresh UI, etc.
        OnBeforeOpen();
 
        // Make visible but non-interactive during transition
        gameObject.SetActive(true); // Ensure the GO is active for coroutines
        SetCanvasGroupState(0f, false, true); // alpha 0, not interactable, blocks raycasts
 
        // Play audio
        PlaySound(_openSound);
 
        // Run animation
        _activeTransition = StartCoroutine(OpenRoutine());
    }
 
    /// <summary>
    /// Closes this menu. Follows the lifecycle:
    ///   OnBeforeClose() → Animation → OnAfterClose() → Event
    /// </summary>
    public void Close()
    {
        if (!IsOpen || IsTransitioning) return;
 
        StopActiveTransition();
 
        OnBeforeClose();
 
        // Disable interaction immediately
        SetCanvasGroupState(CanvasGroup.alpha, false, false);
 
        PlaySound(_closeSound);
 
        _activeTransition = StartCoroutine(CloseRoutine());
    }
 
    /// <summary>
    /// Immediately opens the menu without any animation. 
    /// Useful for initial setup or restoring state.
    /// </summary>
    public void OpenImmediate()
    {
        if (IsOpen) return;
        StopActiveTransition();
 
        OnBeforeOpen();
 
        gameObject.SetActive(true);
        SetCanvasGroupState(1f, true, true);
        IsOpen = true;
 
        OnAfterOpen();
        OnMenuOpened?.Invoke();
    }
 
    /// <summary>
    /// Immediately closes the menu without any animation.
    /// </summary>
    public void CloseImmediate()
    {
        if (!IsOpen) return;
        StopActiveTransition();
 
        OnBeforeClose();
 
        SetCanvasGroupState(0f, false, false);
        IsOpen = false;
 
        OnAfterClose();
        OnMenuClosed?.Invoke();
    }
 
    // =========================================================================
    // ANIMATION COROUTINES (Override for custom transitions)
    // =========================================================================
 
    /// <summary>
    /// Plays the open animation. Default: alpha fade from 0 → 1.
    /// Override for slide-ins, scale pops, etc.
    /// IMPORTANT: Set IsTransitioning = false and IsOpen = true when done,
    ///            or call base.PlayOpenAnimation() at the end.
    /// </summary>
    protected virtual IEnumerator PlayOpenAnimation()
    {
        yield return FadeCanvasGroup(0f, 1f, _transitionDuration, _transitionCurve);
    }
 
    /// <summary>
    /// Plays the close animation. Default: alpha fade from 1 → 0.
    /// Override for slide-outs, scale shrinks, etc.
    /// </summary>
    protected virtual IEnumerator PlayCloseAnimation()
    {
        yield return FadeCanvasGroup(1f, 0f, _transitionDuration, _transitionCurve);
    }
 
    // =========================================================================
    // LIFECYCLE HOOKS (Override in subclasses)
    // =========================================================================
 
    /// <summary>Called before the open animation starts. Refresh UI data here.</summary>
    protected virtual void OnBeforeOpen() { }
 
    /// <summary>Called after the open animation finishes. Start accepting input here.</summary>
    protected virtual void OnAfterOpen() { }
 
    /// <summary>Called before the close animation starts. Save state here if needed.</summary>
    protected virtual void OnBeforeClose() { }
 
    /// <summary>Called after the close animation finishes. Clean up here.</summary>
    protected virtual void OnAfterClose() { }
 
    /// <summary>
    /// Called when the player presses Back/Escape while this menu is on top.
    /// Default: closes the menu (if AllowBackClose is true).
    /// Override to show confirmation dialogs, save-before-exit, etc.
    /// </summary>
    public virtual void OnBackPressed()
    {
        if (_allowBackClose)
        {
            UIMenuManager.Instance?.Pop();
        }
    }
 
    /// <summary>
    /// Called when this menu moves from the top of the stack because a new menu
    /// was pushed on top of it. Use to pause animations, dim the menu, etc.
    /// </summary>
    public virtual void OnLostFocus()
    {
        // Default: disable interaction but stay visible
        CanvasGroup.interactable = false;
    }
 
    /// <summary>
    /// Called when this menu returns to the top of the stack (the menu above it was popped).
    /// Use to resume animations, refresh data, etc.
    /// </summary>
    public virtual void OnRegainedFocus()
    {
        // Default: re-enable interaction
        CanvasGroup.interactable = true;
    }
 
    // =========================================================================
    // INTERNAL HELPERS
    // =========================================================================
 
    private IEnumerator OpenRoutine()
    {
        IsTransitioning = true;
 
        yield return PlayOpenAnimation();
 
        // Finalize state
        SetCanvasGroupState(1f, true, true);
        IsOpen = true;
        IsTransitioning = false;
        _activeTransition = null;
 
        OnAfterOpen();
        OnMenuOpened?.Invoke();
    }
 
    private IEnumerator CloseRoutine()
    {
        IsTransitioning = true;
 
        yield return PlayCloseAnimation();
 
        // Finalize state
        SetCanvasGroupState(0f, false, false);
        IsOpen = false;
        IsTransitioning = false;
        _activeTransition = null;
 
        OnAfterClose();
        OnMenuClosed?.Invoke();
    }
 
    /// <summary>
    /// Utility: smoothly interpolates CanvasGroup.alpha between two values.
    /// Uses unscaledDeltaTime so it works even when Time.timeScale == 0 (paused).
    /// </summary>
    protected IEnumerator FadeCanvasGroup(float from, float to, float duration, AnimationCurve curve = null)
    {
        if (duration <= 0f)
        {
            CanvasGroup.alpha = to;
            yield break; // No animation, jump to final state
        }
 
        float elapsed = 0f; // Time since the start of the animation
        CanvasGroup.alpha = from; // Ensure starting alpha is set

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime; // unscaled: works during pause!
            float t = Mathf.Clamp01(elapsed / duration); // Normalized time [0,1], clamped to avoid overshooting ,To put it simply, it ensures that t never exceeds 1, which prevents the animation from going beyond its intended duration and final state. and the use 
            float evaluated = curve != null ? curve.Evaluate(t) : t; // to put it simply, this line checks if a custom animation curve is provided. If it is, it evaluates the curve at the normalized time t to get the eased value. If no curve is provided, it just uses t directly for a linear interpolation. This allows for more natural and visually appealing transitions by applying easing functions to the animation.
            CanvasGroup.alpha = Mathf.LerpUnclamped(from, to, evaluated);
            yield return null; // wait for next frame
        }
 
        CanvasGroup.alpha = to; // Ensure final alpha is set (in case of any floating-point inaccuracies)
    }
 
    /// <summary>Sets CanvasGroup alpha, interactable, and blocksRaycasts in one call.</summary>
    protected void SetCanvasGroupState(float alpha, bool interactable, bool blocksRaycasts)
    {
        CanvasGroup.alpha = alpha;
        CanvasGroup.interactable = interactable;
        CanvasGroup.blocksRaycasts = blocksRaycasts;
    }
 
    private void StopActiveTransition()
    {
        if (_activeTransition != null)
        {
            StopCoroutine(_activeTransition);
            _activeTransition = null;
            IsTransitioning = false;
        }
    }
 
    private void PlaySound(AudioClip clip)
    {
        if (clip == null) return;
        // Using PlayClipAtPoint as a simple fire-and-forget. 
        // Replace with your audio system (e.g., AudioManager.Instance.PlayUI(clip))
        AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position);
    }
}