using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class PopUpDialog : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform dialogTransform;
    [SerializeField] private CanvasGroup dialogCanvasGroup;
    [SerializeField] private Image backgroundOverlay; // Dark overlay behind dialog
    
    [Header("Animation Settings")]
    [SerializeField] private float showDuration = 0.35f;
    [SerializeField] private float hideDuration = 0.2f;
    [SerializeField] private float overlayAlpha = 0.6f;
    
    private Sequence currentSequence;

    [ContextMenu("Show Dialog")]
    public void Show()
    {
        currentSequence?.Kill();
        gameObject.SetActive(true);
        
        // Reset state
        dialogTransform.localScale = Vector3.one * 0.5f;
        dialogCanvasGroup.alpha = 0f;
        backgroundOverlay.color = new Color(0, 0, 0, 0);
        
        currentSequence = DOTween.Sequence()
            // Background overlay fades in
            .Append(backgroundOverlay.DOFade(overlayAlpha, showDuration * 0.6f)
                .SetEase(Ease.OutQuad))
            // Dialog scales up with overshoot (starts slightly after overlay)
            .Insert(0.05f, dialogTransform.DOScale(Vector3.one, showDuration)
                .SetEase(Ease.OutBack, overshoot: 1.5f))
            // Dialog fades in simultaneously
            .Insert(0.05f, dialogCanvasGroup.DOFade(1f, showDuration * 0.6f)
                .SetEase(Ease.OutQuad))
            .OnComplete(() => {
                dialogCanvasGroup.interactable = true;
                dialogCanvasGroup.blocksRaycasts = true;
            })
            .SetUpdate(true);
    }

    [ContextMenu("Hide Dialog")]
    public void Hide()
    {
        currentSequence?.Kill();
        dialogCanvasGroup.interactable = false;
        
        currentSequence = DOTween.Sequence()
            .Append(dialogTransform.DOScale(Vector3.one * 0.8f, hideDuration)
                .SetEase(Ease.InBack, overshoot: 1.2f))
            .Join(dialogCanvasGroup.DOFade(0f, hideDuration)
                .SetEase(Ease.InQuad))
            .Join(backgroundOverlay.DOFade(0f, hideDuration * 1.2f)
                .SetEase(Ease.InQuad))
            .OnComplete(() => gameObject.SetActive(false))
            .SetUpdate(true);
    }
    
    private void OnDisable() => currentSequence?.Kill();
}