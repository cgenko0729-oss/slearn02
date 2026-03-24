using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class DamageFlash : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image flashOverlay; // Full-screen red Image
    [SerializeField] private RectTransform uiContainer; // Parent of all UI for shake
    
    [Header("Flash Settings")]
    [SerializeField] private Color flashColor = new Color(1f, 0f, 0f, 0.3f);
    [SerializeField] private float flashDuration = 0.15f;
    [SerializeField] private int flashCount = 2;
    
    [Header("Shake Settings")]
    [SerializeField] private float shakeStrength = 15f;
    [SerializeField] private float shakeDuration = 0.3f;
    
    private Sequence damageSequence;
    
    public void PlayDamageEffect()
    {
        damageSequence?.Kill(complete: true); // Complete any existing
        
        flashOverlay.color = Color.clear;
        flashOverlay.gameObject.SetActive(true);
        
        damageSequence = DOTween.Sequence()
            // Red flash
            .Append(flashOverlay.DOColor(flashColor, flashDuration * 0.3f))
            .Append(flashOverlay.DOColor(Color.clear, flashDuration * 0.7f))
            .SetLoops(flashCount, LoopType.Restart)
            
            // UI shake (plays simultaneously with first flash)
            .Insert(0f, uiContainer.DOShakeAnchorPos(
                shakeDuration, 
                strength: shakeStrength,
                vibrato: 20,
                randomness: 90,
                snapping: false,
                fadeOut: true))
            
            .OnComplete(() => flashOverlay.gameObject.SetActive(false));
    }
    
    /// <summary>
    /// Lighter version for "invalid input" feedback
    /// </summary>
    public void PlayErrorShake(RectTransform target)
    {
        target.DOKill();
        target.DOShakeAnchorPos(0.3f, 
            strength: new Vector2(10, 0), // Horizontal only
            vibrato: 15,
            randomness: 0, // Pure horizontal
            fadeOut: true);
    }
}