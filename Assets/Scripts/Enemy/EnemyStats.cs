using System;
using UnityEngine;

/// <summary>
/// 负责敌人核心数值数据（生命、韧性、阶级与 SO 数据加载）
/// </summary>
public class EnemyStats : MonoBehaviour
{
    [Header("敌人阶级分类")]
    public EnemyRank rank = EnemyRank.Normal;

    [Header("数据驱动 Asset")]
    public CharacterStatsSO statsAsset;

    [Header("运行时属性")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float maxToughness = 100f;
    public float currentToughness;

    public event Action<float, float, float, float> OnStatsChanged; // currentHP, maxHP, currentTough, maxTough
    public event Action OnDeath;
    public event Action OnPoiseBroken;

    private bool isDead;

    private void Awake()
    {
        if (statsAsset != null)
        {
            maxHealth = statsAsset.maxHealth;
            maxToughness = statsAsset.maxToughness;
        }

        currentHealth = maxHealth;
        currentToughness = maxToughness;
    }

    private void Start()
    {
        NotifyStatsChanged();
    }

    public void ApplyDamage(float damage, float toughnessDamage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentToughness -= toughnessDamage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        currentToughness = Mathf.Clamp(currentToughness, 0, maxToughness);

        NotifyStatsChanged();

        if (currentHealth <= 0)
        {
            isDead = true;
            OnDeath?.Invoke();
            return;
        }

        if (currentToughness <= 0)
        {
            OnPoiseBroken?.Invoke();
        }
    }

    public void ResetToughness()
    {
        if (isDead) return;
        currentToughness = maxToughness;
        NotifyStatsChanged();
    }

    public void NotifyStatsChanged()
    {
        OnStatsChanged?.Invoke(currentHealth, maxHealth, currentToughness, maxToughness);
    }
}