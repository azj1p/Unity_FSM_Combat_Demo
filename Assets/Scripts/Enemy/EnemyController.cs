using UnityEngine;

[RequireComponent(typeof(EnemyStats))]
[RequireComponent(typeof(EnemyVisual))]
[RequireComponent(typeof(EnemyCombat))]
[RequireComponent(typeof(EnemyUI))]
public class EnemyController : MonoBehaviour, IDamageable, IActionValueEntity
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

    [Header("行动值系统配置 (P3-2)")]
    [SerializeField] private float actionSpeed = 20f; // 基准行动速度 (100 / 20 = 5.0 秒一个周期)

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

    // --- IActionValueEntity 接口实现 ---
    public float ActionSpeed => actionSpeed;

    /// <summary>
    /// 行动值蓄满（达到 100）就绪时的出招驱动
    /// </summary>
    public void ExecuteAction()
    {
        if (isDead || IsVulnerable) return;

        // 行动值蓄满时的动作制转译：若玩家在攻击射程内且不在硬直中，驱动进入攻击状态
        if (playerTransform != null)
        {
            float dist = Vector3.Distance(transform.position, playerTransform.position);
            if (dist <= 2.5f && stateMachine != null && attackState != null)
            {
                if (stateMachine.CurrentState != attackState)
                {
                    stateMachine.ChangeState(attackState);
                }
            }
        }
    }
    // ----------------------------------

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

    private void OnEnable()
    {
        // 注册到行动值时间轴系统
        if (ActionValueSystem.Instance != null)
        {
            ActionValueSystem.Instance.RegisterEntity(this);
        }
    }

    private void OnDisable()
    {
        // 离开场景或失活时安全注销
        if (ActionValueSystem.Instance != null)
        {
            ActionValueSystem.Instance.UnregisterEntity(this);
        }
    }

    private void Start()
    {
        // 弥补场景加载首帧单例初始化时序可能带来的漏注
        if (ActionValueSystem.Instance != null)
        {
            ActionValueSystem.Instance.RegisterEntity(this);
        }
    }

    private void OnDestroy()
    {
        if (ActionValueSystem.Instance != null)
        {
            ActionValueSystem.Instance.UnregisterEntity(this);
        }

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

        // P3-2 核心机制闭环：破韧瘫痪延后行动值 25%（拉长出招间隔与虚弱打桩窗口）
        if (ActionValueSystem.Instance != null)
        {
            ActionValueSystem.Instance.DelayAction(this, 0.25f);
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

        // 死亡后立即移出行动序列，避免死后空转
        if (ActionValueSystem.Instance != null)
        {
            ActionValueSystem.Instance.UnregisterEntity(this);
        }

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