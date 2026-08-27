using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 负责血条与韧性条的显示与刷新
/// </summary>
public class EnemyUI : MonoBehaviour
{
    [SerializeField] private Slider healthBar;
    [SerializeField] private Slider toughnessBar;

    public void UpdateBars(float currentHealth, float maxHealth, float currentToughness, float maxToughness)
    {
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }
        if (toughnessBar != null)
        {
            toughnessBar.maxValue = maxToughness;
            toughnessBar.value = currentToughness;
        }
    }

    public void HideUI()
    {
        if (healthBar != null) healthBar.gameObject.SetActive(false);
        if (toughnessBar != null) toughnessBar.gameObject.SetActive(false);
    }
}