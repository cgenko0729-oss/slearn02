using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class ScreenTransition : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup transitionOverlay;
    [SerializeField] private Image overlayImage;
    
    [Header("Settings")]
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float fadeOutDuration = 0.4f;
    [SerializeField] private float holdDuration = 0.2f; // Time at full black
    
    private Sequence transitionSequence;
    
    /// <summary>
    /// Execute a fade-to-black transition with a callback at the midpoint
    /// (where you load the new scene/swap content).
    /// </summary>
    public void FadeTransition(Action onMidpoint, Action onComplete = null)
    {
        transitionSequence?.Kill();
        
        transitionOverlay.gameObject.SetActive(true);
        transitionOverlay.alpha = 0f;
        transitionOverlay.blocksRaycasts = true;
        
        transitionSequence = DOTween.Sequence()
            // Fade to black
            .Append(transitionOverlay.DOFade(1f, fadeInDuration)
                .SetEase(Ease.InQuad))
            
            // Hold at black (important: gives time for scene load)
            .AppendInterval(holdDuration)
            
            // Execute midpoint callback (load scene, swap content)
            .AppendCallback(() => onMidpoint?.Invoke())
            
            // Small extra hold for content to settle
            .AppendInterval(0.1f)
            
            // Fade back in
            .Append(transitionOverlay.DOFade(0f, fadeOutDuration)
                .SetEase(Ease.OutQuad))
            
            .OnComplete(() => {
                transitionOverlay.blocksRaycasts = false;
                transitionOverlay.gameObject.SetActive(false);
                onComplete?.Invoke();
            })
            .SetUpdate(true);
    }
    
    // Usage:
    // screenTransition.FadeTransition(
    //     onMidpoint: () => SceneManager.LoadScene("NextLevel"),
    //     onComplete: () => GameManager.Instance.StartLevel()
    // );
}