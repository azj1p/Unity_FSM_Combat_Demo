using UnityEngine;

/// <summary>
/// P2-4: 3D 世界空间 AOE 范围与预警动画指示器
/// </summary>
public class WorldAOEIndicator : MonoBehaviour
{
    [Header("视觉组件")]
    [SerializeField] private Renderer indicatorRenderer;
    [SerializeField] private Color warningColor = new Color(1f, 0f, 0f, 0.35f);
    [SerializeField] private Color criticalColor = new Color(1f, 0.2f, 0f, 0.75f);

    private MaterialPropertyBlock mpb;
    private static readonly int BaseMapColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int LegacyColorId = Shader.PropertyToID("_Color");

    private void Awake()
    {
        if (indicatorRenderer == null)
        {
            indicatorRenderer = GetComponentInChildren<Renderer>();
        }
        mpb = new MaterialPropertyBlock();

        // 初始状态保持隐藏
        gameObject.SetActive(false);
    }

    private void Start()
    {
        BindEvents();
    }

    private void OnEnable()
    {
        BindEvents();
        SyncTransformAndRadius();
    }

    private void OnDisable()
    {
        UnbindEvents();
    }

    private void OnDestroy()
    {
        UnbindEvents();
    }

    private void BindEvents()
    {
        var resonance = EnvironmentalResonance.Instance;
        if (resonance == null) return;

        resonance.OnAOEWarningState -= HandleAOEWarningState;
        resonance.OnAOEWarningState += HandleAOEWarningState;

        resonance.OnAOETriggered -= HandleAOETriggered;
        resonance.OnAOETriggered += HandleAOETriggered;
    }

    private void UnbindEvents()
    {
        var resonance = EnvironmentalResonance.Instance;
        if (resonance == null) return;

        resonance.OnAOEWarningState -= HandleAOEWarningState;
        resonance.OnAOETriggered -= HandleAOETriggered;
    }

    /// <summary>
    /// 响应预警状态与倒计时广播
    /// </summary>
    private void HandleAOEWarningState(bool isWarning, float remainingTime)
    {
        if (isWarning)
        {
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
                SyncTransformAndRadius();
            }

            // 预警倒计时越接近 0，颜色闪烁越急促、颜色越深
            var resonance = EnvironmentalResonance.Instance;
            float totalDuration = resonance != null ? resonance.aoeWarningDuration : 1.5f;
            float progress = 1f - Mathf.Clamp01(remainingTime / Mathf.Max(0.01f, totalDuration));

            // 呼吸闪烁频率随时间逼近加速
            float pulseFrequency = Mathf.Lerp(4f, 16f, progress);
            float alphaPulse = (Mathf.Sin(Time.time * pulseFrequency) + 1f) * 0.5f;

            Color currentColor = Color.Lerp(warningColor, criticalColor, progress);
            currentColor.a = Mathf.Lerp(0.2f, 0.7f, alphaPulse);

            SetColor(currentColor);
        }
        else
        {
            if (gameObject.activeSelf)
            {
                gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// AOE 爆发结算后自动隐藏
    /// </summary>
    private void HandleAOETriggered()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 同步真实中心位置与半径
    /// </summary>
    public void SyncTransformAndRadius()
    {
        var resonance = EnvironmentalResonance.Instance;
        if (resonance == null) return;

        // 1. 位置对齐环境中心（高度贴合地面 Y 轴微调防 Z-Fighting）
        Vector3 centerPos = resonance.transform.position;
        centerPos.y = 0.02f;
        transform.position = centerPos;

        // 2. 直径对齐：X 和 Z 缩放 = 2 * aoeRadius
        float diameter = resonance.aoeRadius * 2f;
        transform.localScale = new Vector3(diameter, transform.localScale.y, diameter);
    }

    private void SetColor(Color col)
    {
        if (indicatorRenderer == null) return;

        indicatorRenderer.GetPropertyBlock(mpb);
        mpb.SetColor(BaseMapColorId, col);
        if (indicatorRenderer.sharedMaterial != null && indicatorRenderer.sharedMaterial.HasProperty(LegacyColorId))
        {
            mpb.SetColor(LegacyColorId, col);
        }
        indicatorRenderer.SetPropertyBlock(mpb);
    }
}