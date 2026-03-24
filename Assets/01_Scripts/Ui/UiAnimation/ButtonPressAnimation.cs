using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class ButtonPressAnimation : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Press Settings")]
    [SerializeField] private float pressScale = 0.9f;
    [SerializeField] private float pressDuration = 0.08f;
    
    [Header("Hover Settings")]
    [SerializeField] private float hoverScale = 1.05f;
    [SerializeField] private float hoverDuration = 0.15f;
    
    private Vector3 originalScale;
    private Tween currentTween;
    
    private void Awake()
    {
        originalScale = transform.localScale;
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        currentTween?.Kill();
        currentTween = transform.DOScale(originalScale * pressScale, pressDuration)
            .SetEase(Ease.OutQuad)
            .SetUpdate(true); // SetUpdate(true) = works even when Time.timeScale = 0
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        currentTween?.Kill();
        currentTween = transform.DOScale(originalScale * hoverScale, 0.15f)
            .SetEase(Ease.OutBack); // Bouncy release feels great
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        currentTween?.Kill();
        currentTween = transform.DOScale(originalScale * hoverScale, hoverDuration)
            .SetEase(Ease.OutQuad);
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        currentTween?.Kill();
        currentTween = transform.DOScale(originalScale, hoverDuration)
            .SetEase(Ease.OutQuad);
    }
    
    private void OnDisable()
    {
        currentTween?.Kill();
        transform.localScale = originalScale;
    }
}