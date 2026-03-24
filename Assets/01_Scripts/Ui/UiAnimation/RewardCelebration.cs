using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class RewardCelebration : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform rewardIcon;
    [SerializeField] private CanvasGroup containerGroup;
    [SerializeField] private Image glowEffect;
    [SerializeField] private RectTransform[] particles; // Simple star/sparkle images
    [SerializeField] private TextMeshProUGUI rewardText;
    [SerializeField] private Image backgroundFlash;
    
    public void PlayCelebration()
    {
        containerGroup.alpha = 0f;
        containerGroup.gameObject.SetActive(true);
        
        Sequence celebration = DOTween.Sequence();
        
        // 1. Background flash
        backgroundFlash.color = Color.clear;
        celebration.Append(backgroundFlash.DOColor(
            new Color(1, 1, 1, 0.8f), 0.1f));
        celebration.Append(backgroundFlash.DOColor(
            Color.clear, 0.3f).SetEase(Ease.OutQuad));
        
        // 2. Container fade in
        celebration.Insert(0.05f, containerGroup.DOFade(1f, 0.2f));
        
        // 3. Reward icon: dramatic entrance
        rewardIcon.localScale = Vector3.zero;
        celebration.Insert(0.15f, rewardIcon.DOScale(Vector3.one * 1.3f, 0.4f)
            .SetEase(Ease.OutBack, overshoot: 2f));
        celebration.Insert(0.55f, rewardIcon.DOScale(Vector3.one, 0.2f)
            .SetEase(Ease.InOutQuad));
        
        // 4. Glow pulse
        glowEffect.color = new Color(1, 0.9f, 0.3f, 0f);
        celebration.Insert(0.2f, glowEffect.DOFade(0.6f, 0.3f)
            .SetEase(Ease.OutQuad));
        celebration.Insert(0.5f, glowEffect.DOFade(0f, 0.8f)
            .SetEase(Ease.InOutSine)
            .SetLoops(3, LoopType.Yoyo));
        
        // 5. Glow rotation
        celebration.Insert(0.2f, glowEffect.rectTransform.DORotate(
            new Vector3(0, 0, 360), 3f, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart));
        
        // 6. Particles burst outward
        for (int i = 0; i < particles.Length; i++)
        {
            float angle = (360f / particles.Length) * i;
            float distance = Random.Range(150f, 250f);
            Vector2 direction = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)) * distance;
            
            particles[i].localScale = Vector3.zero;
            particles[i].anchoredPosition = Vector2.zero;
            
            float delay = 0.2f + (i * 0.03f);
            celebration.Insert(delay, particles[i].DOScale(
                Random.Range(0.5f, 1f), 0.3f)
                .SetEase(Ease.OutBack));
            celebration.Insert(delay, particles[i].DOAnchorPos(
                direction, 0.6f)
                .SetEase(Ease.OutQuad));
            celebration.Insert(delay + 0.4f, particles[i].GetComponent<CanvasGroup>()
                .DOFade(0f, 0.3f));
        }
        
        // 7. Reward text
        rewardText.transform.localScale = Vector3.zero;
        celebration.Insert(0.5f, rewardText.transform.DOScale(1f, 0.35f)
            .SetEase(Ease.OutBack));
    }
}