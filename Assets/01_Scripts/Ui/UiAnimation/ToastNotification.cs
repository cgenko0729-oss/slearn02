using UnityEngine;
using TMPro;
using DG.Tweening;

public class ToastNotification : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform toastTransform;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI messageText;
    
    [Header("Settings")]
    [SerializeField] private float slideDistance = 80f;
    [SerializeField] private float enterDuration = 0.35f;
    [SerializeField] private float displayDuration = 2.5f;
    [SerializeField] private float exitDuration = 0.25f;
    
    private Sequence toastSequence;
    
    public void ShowToast(string message)
    {
        toastSequence?.Kill();
        messageText.text = message;
        gameObject.SetActive(true);
        
        // Start position: slightly above final position, invisible
        Vector2 startOffset = new Vector2(0, slideDistance);
        
        toastSequence = DOTween.Sequence()
            .OnStart(() => {
                canvasGroup.alpha = 0f;
                toastTransform.anchoredPosition += startOffset;
            })
            
            // Enter: slide down + fade in
            .Append(toastTransform.DOAnchorPos(
                toastTransform.anchoredPosition - startOffset, enterDuration)
                .SetEase(Ease.OutCubic))
            .Join(canvasGroup.DOFade(1f, enterDuration * 0.7f))
            
            // Stay visible
            .AppendInterval(displayDuration)
            
            // Exit: slide up + fade out
            .Append(toastTransform.DOAnchorPosY(
                toastTransform.anchoredPosition.y + slideDistance * 0.5f, exitDuration)
                .SetEase(Ease.InCubic))
            .Join(canvasGroup.DOFade(0f, exitDuration))
            
            .OnComplete(() => gameObject.SetActive(false))
            .SetUpdate(true);
    }
}