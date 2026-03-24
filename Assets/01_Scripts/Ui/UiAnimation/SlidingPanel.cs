using UnityEngine;
using DG.Tweening;

public class SlidingPanel : MonoBehaviour
{
    [Header("Slide Settings")]
    [SerializeField] private RectTransform panel;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private SlideDirection direction = SlideDirection.Left;
    [SerializeField] private float slideDuration = 0.4f;
    [SerializeField] private float fadeDuration = 0.25f;
    
    public enum SlideDirection { Left, Right, Top, Bottom }
    
    private Vector2 onScreenPos;
    private Vector2 offScreenPos;
    private Sequence currentSequence;
    
    private void Awake()
    {
        // Store the on-screen position (set this in editor)
        onScreenPos = panel.anchoredPosition;
        
        // Calculate off-screen position based on direction
        float panelWidth = panel.rect.width;
        float panelHeight = panel.rect.height;
        
        offScreenPos = direction switch
        {
            SlideDirection.Left  => onScreenPos + new Vector2(-panelWidth - 100, 0),
            SlideDirection.Right => onScreenPos + new Vector2(panelWidth + 100, 0),
            SlideDirection.Top   => onScreenPos + new Vector2(0, panelHeight + 100),
            SlideDirection.Bottom=> onScreenPos + new Vector2(0, -panelHeight - 100),
            _ => onScreenPos
        };
    }

    [ContextMenu("Show Panel")]
    public void Show()
    {
        currentSequence?.Kill();
        gameObject.SetActive(true);
        
        currentSequence = DOTween.Sequence()
            .OnStart(() => {
                panel.anchoredPosition = offScreenPos;
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            })
            .Append(panel.DOAnchorPos(onScreenPos, slideDuration).SetEase(Ease.OutCubic))
            .Join(canvasGroup.DOFade(1f, fadeDuration).SetEase(Ease.OutQuad))
            .OnComplete(() => {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            })
            .SetUpdate(true);
    }
    
    [ContextMenu("Hide Panel")]
    public void Hide()
    {
        currentSequence?.Kill();
        
        currentSequence = DOTween.Sequence()
            .OnStart(() => {
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            })
            .Append(panel.DOAnchorPos(offScreenPos, slideDuration * 0.8f).SetEase(Ease.InCubic))
            .Join(canvasGroup.DOFade(0f, fadeDuration * 0.8f).SetEase(Ease.InQuad))
            .OnComplete(() => gameObject.SetActive(false))
            .SetUpdate(true);
    }
    
    private void OnDisable()
    {
        currentSequence?.Kill();
    }
}