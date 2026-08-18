using UnityEngine;
using UnityEngine.UI;

public class EnemyController : MonoBehaviour, IDamageable
{
    [Header("Stats Asset (数据驱动/可选)")]
    [Tooltip("可选：拖入配置好的 Stats 资产文件；若为空则使用下方默认属性")]
    public CharacterStatsSO statsAsset;

    [Header("Runtime Attributes")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float maxToughness = 100f;
    public float currentToughness;

    [HideInInspector] public bool isVulnerable;
    [HideInInspector] public float vulnerableTimer; // 独立破韧计时器（防止多怪计时污染）
    [HideInInspector] public float attackTimer;     // 独立攻击后摇计时器

    [Header("Environmental Resonance (环境共鸣)")]
    public int resonanceStacks = 0;
    public int maxResonanceStacks = 3;
    public float resonanceDamageBonus = 0.1f;
    public float resonanceInterval = 6.0f;
    public float aoeRadius = 8.0f;
    private float resonanceTimer;

    [Header("UI")]
    public Slider healthBar;
    public Slider toughnessBar;

    [Header("States")]
    public State idleState;
    public State patrolState;
    public State chaseState;
    public State attackState;
    public State vulnerableState;
    public State deadState;

    [HideInInspector] public StateMachine stateMachine;
    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public bool isDead;
    [HideInInspector] public Transform playerTransform;

    // 实例级独立材质与颜色缓存
    private Renderer enemyRenderer;
    private Color originalColor;

    private void Awake()
    {
        stateMachine = GetComponent<StateMachine>();
        rb = GetComponent<Rigidbody>();
        enemyRenderer = GetComponent<Renderer>();

        if (enemyRenderer != null)
        {
            originalColor = enemyRenderer.material.color;
        }

        // 数据驱动：从 SO 资产加载基础属性
        if (statsAsset != null)
        {
            maxHealth = statsAsset.maxHealth;
            maxToughness = statsAsset.maxToughness;
        }

        currentHealth = maxHealth;
        currentToughness = maxToughness;
        resonanceTimer = resonanceInterval;
    }

    private void Start()
    {
        // 缓存玩家引用，消除 Update 中的高频 FindWithTag
        var playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }

        UpdateUI();
    }

    private void Update()
    {
        if (isDead || isVulnerable) return;

        // 环境共鸣计时器与叠层
        resonanceTimer -= Time.deltaTime;
        if (resonanceTimer <= 0f)
        {
            resonanceTimer = resonanceInterval;
            if (resonanceStacks < maxResonanceStacks)
            {
                resonanceStacks++;
                Debug.Log($"【环境共鸣】层数累加: {resonanceStacks}/{maxResonanceStacks} (+{resonanceStacks * resonanceDamageBonus * 100}% 增伤)");

                if (resonanceStacks >= maxResonanceStacks)
                {
                    TriggerResonanceAOE();
                }
            }
        }
    }

    // 独立的破韧视觉变色控制
    public void SetVulnerableVisual(bool enable)
    {
        if (enemyRenderer != null)
        {
            enemyRenderer.material.color = enable ? new Color(0.6f, 0.6f, 0.6f, 1f) : originalColor;
        }
    }

    // 满层 8m AOE 爆发
    public void TriggerResonanceAOE()
    {
        Debug.LogWarning("【环境共鸣爆发】共鸣满层！释放 8m AOE 爆发伤害！");
        if (playerTransform != null)
        {
            float dist = Vector3.Distance(transform.position, playerTransform.position);
            if (dist <= aoeRadius)
            {
                var damageable = playerTransform.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(30f);
                }
            }
        }
        resonanceStacks = 0;
    }

    // 计算共鸣增伤后的最终攻击力
    public float GetCalculatedAttackDamage(float baseDamage)
    {
        return baseDamage * (1f + resonanceStacks * resonanceDamageBonus);
    }

    // IDamageable 接口实现（多态受击）
    public void TakeDamage(float damage, float toughnessDamage)
    {
        if (isDead) return;

        // IDamageModifier 接口解耦伤害倍率计算
        if (stateMachine != null && stateMachine.CurrentState is IDamageModifier modifier)
        {
            damage = modifier.ModifyDamage(damage);
        }

        currentHealth -= damage;
        currentToughness -= toughnessDamage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        currentToughness = Mathf.Clamp(currentToughness, 0, maxToughness);
        UpdateUI();

        Debug.Log($"怪物受击！生命: {currentHealth}/{maxHealth} | 韧性: {currentToughness}/{maxToughness}");

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        if (!isVulnerable && currentToughness <= 0)
        {
            TriggerBreak();
        }
    }

    // 破韧逻辑：打断并清零共鸣
    public void TriggerBreak()
    {
        resonanceStacks = 0;
        resonanceTimer = resonanceInterval;
        Debug.Log("【破韧机制】怪物破韧！共鸣层数已清零重置！");

        if (stateMachine != null && vulnerableState != null)
        {
            stateMachine.ChangeState(vulnerableState);
        }
    }

    public void ResetToughness()
    {
        if (isDead) return;
        currentToughness = maxToughness;
        UpdateUI();
        Debug.Log("【韧性重置】怪物韧性条回满！");
    }

    public void HideUI()
    {
        if (healthBar != null) healthBar.gameObject.SetActive(false);
        if (toughnessBar != null) toughnessBar.gameObject.SetActive(false);
    }

    public void UpdateUI()
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

    public void Die()
    {
        if (isDead) return;
        if (stateMachine != null && deadState != null)
        {
            stateMachine.ChangeState(deadState);
        }
        else
        {
            isDead = true;
            Destroy(gameObject, 0.2f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // 红色线框：8m 共鸣 AOE 范围
        Gizmos.color = new Color(1f, 0f, 0f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, aoeRadius);

        // 黄色线框：近战攻击判定范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 2.0f);
    }
}