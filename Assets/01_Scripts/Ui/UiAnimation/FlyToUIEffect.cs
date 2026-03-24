using UnityEngine;
using DG.Tweening;
using System;

public class FlyToUIEffect : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float flyDuration = 0.7f;
    [SerializeField] private float scatterRadius = 100f;
    [SerializeField] private float startDelay = 0.05f; // Stagger between each item
    
    /// <summary>
    /// Spawn multiple items that scatter then fly to a target UI element.
    /// Call from the world position converted to screen space.
    /// </summary>
    public void SpawnFlyEffect(
        GameObject prefab, 
        RectTransform spawnPoint, 
        RectTransform targetUI, 
        int count, 
        Transform parent,
        Action onEachArrival = null,
        Action onAllComplete = null)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject item = Instantiate(prefab, parent);
            RectTransform rt = item.GetComponent<RectTransform>();
            rt.anchoredPosition = spawnPoint.anchoredPosition;
            
            // Random scatter position
            Vector2 scatterPos = spawnPoint.anchoredPosition + 
                UnityEngine.Random.insideUnitCircle * scatterRadius;
            
            float delay = i * startDelay;
            int index = i;
            
            DOTween.Sequence()
                // Phase 1: Scatter outward
                .Append(rt.DOAnchorPos(scatterPos, 0.2f)
                    .SetEase(Ease.OutQuad))
                .Join(rt.DOScale(1.2f, 0.2f)
                    .SetEase(Ease.OutQuad))
                
                // Phase 2: Brief pause
                .AppendInterval(0.1f + delay)
                
                // Phase 3: Fly to target
                .Append(rt.DOAnchorPos(targetUI.anchoredPosition, flyDuration)
                    .SetEase(Ease.InQuad)) // Accelerate toward target
                .Join(rt.DOScale(0.3f, flyDuration * 0.8f)
                    .SetEase(Ease.InQuad))
                
                .OnComplete(() => {
                    onEachArrival?.Invoke();
                    Destroy(item);
                    
                    // Last item complete
                    if (index == count - 1)
                        onAllComplete?.Invoke();
                });
        }
    }

    /// <summary>
    /// Represents a test method for demonstration or placeholder purposes.
    /// </summary>
    void TestFunc()
    {

    }

}