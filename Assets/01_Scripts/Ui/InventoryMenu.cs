using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class InventoryMenu : UIMenuBase
{   
    [Header("Slide Animation")]
    [SerializeField] private float _slideDistance = 600f;
    [SerializeField] private float _slideDuration = 0.3f;

    protected override IEnumerator PlayOpenAnimation()
    {
        // Slide in from the right while fading
        Vector2 startPos = RectTransform.anchoredPosition + Vector2.right * _slideDistance;
        Vector2 endPos = RectTransform.anchoredPosition;
 
        RectTransform.anchoredPosition = startPos;
 
        float elapsed = 0f;
        while (elapsed < _slideDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / _slideDuration);
 
            RectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            CanvasGroup.alpha = t;
 
            yield return null;
        }
 
        RectTransform.anchoredPosition = endPos;
        CanvasGroup.alpha = 1f;
    }

    protected override IEnumerator PlayCloseAnimation()
    {
        Vector2 startPos = RectTransform.anchoredPosition;
        Vector2 endPos = startPos + Vector2.right * _slideDistance;
 
        float elapsed = 0f;
        while (elapsed < _slideDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / _slideDuration);
 
            RectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            CanvasGroup.alpha = 1f - t;
 
            yield return null;
        }
 
        RectTransform.anchoredPosition = endPos;
        CanvasGroup.alpha = 0f;
    }
 
    protected override void OnBeforeOpen()
    {
        // Populate inventory items from player data
        Debug.Log("Refreshing inventory contents...");
    }



}