using UnityEngine;
using DG.Tweening;

public class IdlePulse : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float minScale = 0.95f;
    [SerializeField] private float maxScale = 1.05f;
    [SerializeField] private float cycleDuration = 1.5f;
    
    [Header("Fade Pulse (optional)")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float minAlpha = 0.5f;
    [SerializeField] private float maxAlpha = 1f;
    
    private Tween scaleTween;
    private Tween alphaTween;
    
    private void OnEnable()
    {
        StartPulse();
    }
    
    public void StartPulse()
    {
        scaleTween?.Kill();
        alphaTween?.Kill();
        
        // Scale pulse
        transform.localScale = Vector3.one * minScale;
        scaleTween = transform.DOScale(maxScale, cycleDuration)
            .SetEase(Ease.InOutSine) // InOutSine = perfect for breathing
            .SetLoops(-1, LoopType.Yoyo);
        
        // Alpha pulse (if canvasGroup assigned)
        if (canvasGroup != null)
        {
            canvasGroup.alpha = minAlpha;
            alphaTween = canvasGroup.DOFade(maxAlpha, cycleDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }
    }
    
    public void StopPulse()
    {
        scaleTween?.Kill();
        alphaTween?.Kill();
        transform.localScale = Vector3.one;
        if (canvasGroup != null) canvasGroup.alpha = 1f;
    }
    
    private void OnDisable()
    {
        scaleTween?.Kill();
        alphaTween?.Kill();
    }
}