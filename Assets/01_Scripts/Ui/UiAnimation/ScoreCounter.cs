using UnityEngine;
using TMPro;
using DG.Tweening;

public class ScoreCounter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private RectTransform scoreTransform;

    [Header("Settings")]
    [SerializeField] private float countDuration = 1f;
    [SerializeField] private string format = "N0"; // Number format (N0 = no decimals with commas)

    private int currentDisplayedScore;
    private Tween countTween;
    private Tween punchTween;

    public void SetScore(int newScore, bool animate = true)
    {
        countTween?.Kill();
        punchTween?.Kill();

        if (!animate)
        {
            currentDisplayedScore = newScore;
            scoreText.text = newScore.ToString(format);
            return;
        }

        int startScore = currentDisplayedScore;

        // Animate the number counting up
        countTween = DOTween.To(
            getter: () => currentDisplayedScore,
            setter: (value) => {
                currentDisplayedScore = value;
                scoreText.text = value.ToString(format);
            },
            endValue: newScore,
            duration: countDuration
        ).SetEase(Ease.OutCubic);

        // Punch scale for visual emphasis
        punchTween = scoreTransform.DOPunchScale(
            punch: Vector3.one * 0.2f,
            duration: 0.4f,
            vibrato: 5,
            elasticity: 0.5f
        );
    }

    /// <summary>
    /// For adding score incrementally (e.g., collecting coins)
    /// </summary>
    public void AddScore(int amount)
    {
        SetScore(currentDisplayedScore + amount);
    }

    [ContextMenu("Test Add Score")]
    public void TestAddScore()
    {
                AddScore(100);
    }

    [ContextMenu("Test Reduce Score")]
    public void TestReduceScore()
    {
                AddScore(-50);
    }

}