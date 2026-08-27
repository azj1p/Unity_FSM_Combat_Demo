using UnityEngine;

/// <summary>
/// 负责敌人材质改色与破韧视觉表现
/// </summary>
public class EnemyVisual : MonoBehaviour
{
    [Header("视觉组件与配置")]
    [SerializeField] private Renderer enemyRenderer;
    [Tooltip("破韧虚弱状态下的高亮颜色（默认亮金/黄光，可针对不同怪自定义）")]
    [SerializeField] private Color vulnerableColor = new Color(1f, 0.85f, 0.3f, 1f);

    private MaterialPropertyBlock mpb;
    private Color originalColor = Color.white;
    private static readonly int BaseMapColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int LegacyColorId = Shader.PropertyToID("_Color");

    private void Awake()
    {
        if (enemyRenderer == null) enemyRenderer = GetComponent<Renderer>();
        mpb = new MaterialPropertyBlock();

        if (enemyRenderer != null && enemyRenderer.sharedMaterial != null)
        {
            if (enemyRenderer.sharedMaterial.HasProperty(BaseMapColorId))
                originalColor = enemyRenderer.sharedMaterial.GetColor(BaseMapColorId);
            else if (enemyRenderer.sharedMaterial.HasProperty(LegacyColorId))
                originalColor = enemyRenderer.sharedMaterial.GetColor(LegacyColorId);
        }
    }

    public void SetVulnerableVisual(bool enable)
    {
        if (enemyRenderer == null) return;

        enemyRenderer.GetPropertyBlock(mpb);
        Color targetColor = enable ? vulnerableColor : originalColor;
        mpb.SetColor(BaseMapColorId, targetColor);
        if (enemyRenderer.sharedMaterial != null && enemyRenderer.sharedMaterial.HasProperty(LegacyColorId))
        {
            mpb.SetColor(LegacyColorId, targetColor);
        }
        enemyRenderer.SetPropertyBlock(mpb);
    }
}