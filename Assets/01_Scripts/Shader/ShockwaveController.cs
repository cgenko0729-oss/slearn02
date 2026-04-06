using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Renderer))]
public class ShockwaveController : MonoBehaviour
{
    [Header("Lifetime")]
    [SerializeField] private float duration = 0.35f;
    [SerializeField] private float startScale = 0.2f;
    [SerializeField] private float endScale = 3.5f;

    [Header("Shader Params")]
    [SerializeField] private float startRadius = 0.02f;
    [SerializeField] private float endRadius = 0.48f;

    [SerializeField] private float startWidth = 0.06f;
    [SerializeField] private float endWidth = 0.12f;

    [SerializeField] private float startDistortion = 0.18f;
    [SerializeField] private float endDistortion = 0.0f;

    [SerializeField] private float startOpacity = 1.0f;
    [SerializeField] private float endOpacity = 0.0f;

    [SerializeField] private float startEdgeGlow = 3.0f;
    [SerializeField] private float endEdgeGlow = 0.0f;

    [Header("Animation Curves")]
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve radiusCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve distortionCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    [SerializeField] private AnimationCurve opacityCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    [Header("Orientation")]
    [SerializeField] private bool billboardToCamera = true;
    [SerializeField] private bool alignToSurfaceNormal = false;

    private Renderer cachedRenderer;
    private MaterialPropertyBlock mpb;
    private Camera mainCam;

    private float timer;
    private bool isPlaying;
    private Vector3 surfaceNormal = Vector3.up;

    private static readonly int RingRadiusID = Shader.PropertyToID("_RingRadius");
    private static readonly int RingWidthID = Shader.PropertyToID("_RingWidth");
    private static readonly int DistortionStrengthID = Shader.PropertyToID("_DistortionStrength");
    private static readonly int OpacityID = Shader.PropertyToID("_Opacity");
    private static readonly int EdgeGlowID = Shader.PropertyToID("_EdgeGlow");

    private void Awake()
    {
        cachedRenderer = GetComponent<Renderer>();
        mpb = new MaterialPropertyBlock();
        mainCam = Camera.main;
    }

    private void OnEnable()
    {
        Play();
    }

    public void Play()
    {
        timer = 0f;
        isPlaying = true;
        ApplyVisual(0f);
    }

    public void Play(Vector3 normal)
    {
        surfaceNormal = normal.normalized;
        timer = 0f;
        isPlaying = true;
        UpdateOrientation();
        ApplyVisual(0f);
    }

    private void Update()
    {
        if (!isPlaying) return;

        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / duration);

        UpdateOrientation();
        ApplyVisual(t);

        if (timer >= duration)
        {
            Destroy(gameObject);
            // 如果你有对象池，这里改成回收
            // gameObject.SetActive(false);
        }
    }

    private void UpdateOrientation()
    {
        if (billboardToCamera && mainCam != null)
        {
            Vector3 camForward = mainCam.transform.forward;
            transform.rotation = Quaternion.LookRotation(camForward, Vector3.up);
        }

        if (alignToSurfaceNormal)
        {
            transform.rotation = Quaternion.LookRotation(surfaceNormal);
        }
    }

    private void ApplyVisual(float t)
    {
        float scaleT = scaleCurve.Evaluate(t);
        float radiusT = radiusCurve.Evaluate(t);
        float distortionT = distortionCurve.Evaluate(t);
        float opacityT = opacityCurve.Evaluate(t);

        float scale = Mathf.Lerp(startScale, endScale, scaleT);
        float ringRadius = Mathf.Lerp(startRadius, endRadius, radiusT);
        float ringWidth = Mathf.Lerp(startWidth, endWidth, radiusT);
        float distortion = Mathf.Lerp(startDistortion, endDistortion, distortionT);
        float opacity = Mathf.Lerp(startOpacity, endOpacity, opacityT);
        float edgeGlow = Mathf.Lerp(startEdgeGlow, endEdgeGlow, opacityT);

        transform.localScale = Vector3.one * scale;

        cachedRenderer.GetPropertyBlock(mpb);
        mpb.SetFloat(RingRadiusID, ringRadius);
        mpb.SetFloat(RingWidthID, ringWidth);
        mpb.SetFloat(DistortionStrengthID, distortion);
        mpb.SetFloat(OpacityID, opacity);
        mpb.SetFloat(EdgeGlowID, edgeGlow);
        cachedRenderer.SetPropertyBlock(mpb);
    }
}