using UnityEngine;
using UnityEngine.UI;

public class EnemyController : MonoBehaviour, IDamageable
{
    [Header("Enemy Classification (敌人分类)")]
    [Tooltip("Normal(小怪:破韧/击杀-1层) | Elite(精英:重置为0) | Boss(首领:重置为0+推条25%)")]
    public EnemyRank rank = EnemyRank.Normal;

    [Header("Stats Asset (数据驱动)")]
    public CharacterStatsSO statsAsset;

    [Header("Runtime Attributes")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float maxToughness = 100f;
    public float currentToughness;

    [HideInInspector] public bool isVulnerable;
    [HideInInspector] public float vulnerableTimer;
    [HideInInspector] public float attackTimer;

    // 单一事实源：共鸣层数统一通过单例读取，消除冗余字段
    public int resonanceStacks => EnvironmentalResonance.Instance != null ? EnvironmentalResonance.Instance.resonanceStacks : 0;

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

    // 工业级材质改色：MaterialPropertyBlock 零内存分配、不破坏合批
    private Renderer enemyRenderer;
    private MaterialPropertyBlock mpb;
    private Color originalColor = Color.white;
    private static readonly int BaseMapColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int LegacyColorId = Shader.PropertyToID("_Color");

    private void Awake()
    {
        stateMachine = GetComponent<StateMachine>();
        rb = GetComponent<Rigidbody>();
        enemyRenderer = GetComponent<Renderer>();
        mpb = new MaterialPropertyBlock();

        if (enemyRenderer != null && enemyRenderer.sharedMaterial != null)
        {
            originalColor = enemyRenderer.sharedMaterial.color;
        }

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
        var playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }

        UpdateUI();
    }

    // 修复 New 5.1：使用 MaterialPropertyBlock 改色
    public void SetVulnerableVisual(bool enable)
    {
        if (enemyRenderer == null) return;
        enemyRenderer.GetPropertyBlock(mpb);
        Color targetColor = enable ? new Color(0.6f, 0.2f, 0.2f, 1f) : originalColor;
        mpb.SetColor(BaseMapColorId, targetColor);
        mpb.SetColor(LegacyColorId, targetColor);
        enemyRenderer.SetPropertyBlock(mpb);
    }

    // 委托给 EnvironmentalResonance 计算增伤
    public float GetCalculatedAttackDamage(float baseDamage)
    {
        if (EnvironmentalResonance.Instance != null)
        {
            return baseDamage * EnvironmentalResonance.Instance.GetDamageBonusMultiplier();
        }
        return baseDamage;
    }

    public void TakeDamage(float damage, float toughnessDamage = 0f)
    {
        if (isDead) return;

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

    // 破韧逻辑：通知环境共鸣系统处理层数变化
    public void TriggerBreak()
    {
        if (EnvironmentalResonance.Instance != null)
        {
            EnvironmentalResonance.Instance.OnEnemyBrokenOrKilled(rank, false);
        }

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
        isDead = true;

        if (EnvironmentalResonance.Instance != null)
        {
            EnvironmentalResonance.Instance.OnEnemyBrokenOrKilled(rank, true);
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