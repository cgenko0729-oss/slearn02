using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CardFlip : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform cardTransform;
    [SerializeField] private GameObject frontFace;
    [SerializeField] private GameObject backFace;
    
    [Header("Settings")]
    [SerializeField] private float flipDuration = 0.6f;
    
    private bool isFront = false;
    private Sequence flipSequence;
    
    private void Awake()
    {
        // Start with back face showing
        frontFace.SetActive(false);
        backFace.SetActive(true);
    }
    
    public void Flip()
    {
        flipSequence?.Kill();
        bool toFront = !isFront;
        
        flipSequence = DOTween.Sequence()
            // Phase 1: Rotate to 90 degrees (card becomes edge-on, invisible)
            .Append(cardTransform.DORotate(
                new Vector3(0, 90, 0), flipDuration * 0.5f)
                .SetEase(Ease.InQuad))
            
            // At the midpoint, swap faces
            .AppendCallback(() => {
                frontFace.SetActive(toFront);
                backFace.SetActive(!toFront);
            })
            
            // Phase 2: Rotate from -90 to 0 (reveals new face)
            .Append(cardTransform.DORotate(
                new Vector3(0, 0, 0), flipDuration * 0.5f)
                .From(new Vector3(0, -90, 0))
                .SetEase(Ease.OutQuad))
            
            .OnComplete(() => isFront = toFront);
    }
    
    /// <summary>
    /// Gacha-style dramatic reveal with buildup
    /// </summary>
    public void DramaticReveal()
    {
        flipSequence?.Kill();
        
        flipSequence = DOTween.Sequence()
            // Slight wobble buildup
            .Append(cardTransform.DOShakeRotation(0.5f, 
                strength: new Vector3(0, 5, 2), 
                vibrato: 10, 
                randomness: 45))
            
            // Quick scale up
            .Append(cardTransform.DOScale(1.15f, 0.15f)
                .SetEase(Ease.OutQuad))
            
            // The flip
            .Append(cardTransform.DORotate(
                new Vector3(0, 90, 0), 0.2f)
                .SetEase(Ease.InQuad))
            .AppendCallback(() => {
                frontFace.SetActive(true);
                backFace.SetActive(false);
            })
            .Append(cardTransform.DORotate(
                new Vector3(0, 0, 0), 0.3f)
                .From(new Vector3(0, -90, 0))
                .SetEase(Ease.OutBack))
            
            // Settle back to normal scale
            .Append(cardTransform.DOScale(1f, 0.2f)
                .SetEase(Ease.OutQuad))
            
            .OnComplete(() => isFront = true);
    }
    
    private void OnDisable() => flipSequence?.Kill();
}