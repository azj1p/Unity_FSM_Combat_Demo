using UnityEngine;

[RequireComponent(typeof(EnemyStats))]
[RequireComponent(typeof(EnemyVisual))]
[RequireComponent(typeof(EnemyCombat))]
[RequireComponent(typeof(EnemyUI))]
public class EnemyController : MonoBehaviour, IDamageable
{
    [Header("子系统组件引用")]
    [HideInInspector] public EnemyStats stats;
    [HideInInspector] public EnemyVisual visual;
    [HideInInspector] public EnemyCombat combat;
    [HideInInspector] public EnemyUI enemyUI;

    [Header("States")]
    public State idleState;
    public State patrolState;
    public State chaseState;
    public State attackState;
    public State vulnerableState;
    public State deadState;

    [HideInInspector] public StateMachine stateMachine;
    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public float vulnerableTimer;
    [HideInInspector] public float attackTimer;

    // 单一事实源与状态标记
    public bool isVulnerable => stateMachine != null && stateMachine.CurrentState == vulnerableState;
    public bool IsVulnerable => isVulnerable;
    [HideInInspector] public bool isDead;

    // Facade 门面映射：保证所有现有状态类读写不受任何影响
    public Transform playerTransform => combat != null ? combat.playerTransform : null;
    public EnemyRank rank => stats != null ? stats.rank : EnemyRank.Normal;
    public float currentHealth => stats != null ? stats.currentHealth : 0f;
    public float maxHealth => stats != null ? stats.maxHealth : 100f;
    public float currentToughness => stats != null ? stats.currentToughness : 0f;
    public float maxToughness => stats != null ? stats.maxToughness : 100f;
    public int resonanceStacks => EnvironmentalResonance.Instance != null ? EnvironmentalResonance.Instance.resonanceStacks : 0;

    private void Awake()
    {
        stateMachine = GetComponent<StateMachine>();
        rb = GetComponent<Rigidbody>();
        stats = GetComponent<EnemyStats>();
        visual = GetComponent<EnemyVisual>();
        combat = GetComponent<EnemyCombat>();
        enemyUI = GetComponent<EnemyUI>();

        // P1-3: 校验 Boss 配置
        if (stats != null && stats.rank == EnemyRank.Normal && gameObject.name.ToLower().Contains("boss"))
        {
            Debug.LogWarning($"【EnemyController】[{gameObject.name}] 名称包含 Boss 但当前等级仍为 Normal，请检查 Inspector 配置！");
        }

        // 订阅数据事件联动
        if (stats != null)
        {
            stats.OnStatsChanged += HandleStatsChanged;
            stats.OnPoiseBroken += HandlePoiseBroken;
            stats.OnDeath += HandleDeath;
        }
    }

    private void OnDestroy()
    {
        if (stats != null)
        {
            stats.OnStatsChanged -= HandleStatsChanged;
            stats.OnPoiseBroken -= HandlePoiseBroken;
            stats.OnDeath -= HandleDeath;
        }
    }

    private void HandleStatsChanged(float hp, float maxHp, float tough, float maxTough)
    {
        if (enemyUI != null) enemyUI.UpdateBars(hp, maxHp, tough, maxTough);
    }

    private void HandlePoiseBroken()
    {
        if (!IsVulnerable) TriggerBreak();
    }

    private void HandleDeath()
    {
        Die();
    }

    public void TakeDamage(float damage, float toughnessDamage = 0f)
    {
        if (isDead) return;

        if (stateMachine != null && stateMachine.CurrentState is IDamageModifier modifier)
        {
            damage = modifier.ModifyDamage(damage);
        }

        if (stats != null)
        {
            stats.ApplyDamage(damage, toughnessDamage);
        }
    }

    public void TriggerBreak()
    {
        if (EnvironmentalResonance.Instance != null && stats != null)
        {
            EnvironmentalResonance.Instance.OnEnemyBrokenOrKilled(stats.rank, false);
        }

        if (stateMachine != null && vulnerableState != null)
        {
            stateMachine.ChangeState(vulnerableState);
        }
    }

    public void ResetToughness()
    {
        if (stats != null) stats.ResetToughness();
    }

    public void SetVulnerableVisual(bool enable)
    {
        if (visual != null) visual.SetVulnerableVisual(enable);
    }

    public float GetCalculatedAttackDamage(float baseDamage)
    {
        if (combat != null) return combat.CalculateOutgoingDamage(baseDamage);
        return baseDamage;
    }

    public void HideUI()
    {
        if (enemyUI != null) enemyUI.HideUI();
    }

    public void UpdateUI()
    {
        if (stats != null) stats.NotifyStatsChanged();
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        if (EnvironmentalResonance.Instance != null && stats != null)
        {
            EnvironmentalResonance.Instance.OnEnemyBrokenOrKilled(stats.rank, true);
        }

        if (stateMachine != null && deadState != null)
        {
            stateMachine.ChangeState(deadState);
        }
        else
        {
            Destroy(gameObject, 0.2f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 2.0f);
    }
}