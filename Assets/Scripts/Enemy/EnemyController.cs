using UnityEngine;
using UnityEngine.UI;

public class EnemyController : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Toughness")]
    public float maxToughness = 100f;
    public float currentToughness;
    [HideInInspector] public bool isVulnerable;

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
    [HideInInspector] public bool isDead;
    [HideInInspector] public Transform playerTransform;

    // 独立管理每个怪物的材质和初始颜色，防止多个怪物共享状态时颜色错乱
    private Renderer enemyRenderer;
    private Color originalColor;

    private void Start()
    {
        currentHealth = maxHealth;
        currentToughness = maxToughness;
        resonanceTimer = resonanceInterval;
        stateMachine = GetComponent<StateMachine>();

        // 记录当前怪物的独立材质与初始颜色
        enemyRenderer = GetComponent<Renderer>();
        if (enemyRenderer != null)
        {
            originalColor = enemyRenderer.material.color;
        }

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

        resonanceTimer -= Time.deltaTime;
        if (resonanceTimer <= 0f)
        {
            resonanceTimer = resonanceInterval;
            if (resonanceStacks < maxResonanceStacks)
            {
                resonanceStacks++;
                Debug.Log($"【环境共鸣】层数上升: {resonanceStacks}/{maxResonanceStacks} (+{resonanceStacks * resonanceDamageBonus * 100}% 增伤)");

                if (resonanceStacks >= maxResonanceStacks)
                {
                    TriggerResonanceAOE();
                }
            }
        }
    }

    // 独立的破韧视觉变色控制（安全可靠）
    public void SetVulnerableVisual(bool enable)
    {
        if (enemyRenderer != null)
        {
            enemyRenderer.material.color = enable ? new Color(0.6f, 0.6f, 0.6f, 1f) : originalColor;
        }
    }

    public void TriggerResonanceAOE()
    {
        Debug.LogWarning("【环境共鸣爆发】共鸣满层！释放 8m AOE 爆发技能！");
        if (playerTransform != null)
        {
            float dist = Vector3.Distance(transform.position, playerTransform.position);
            if (dist <= aoeRadius)
            {
                var player = playerTransform.GetComponent<PlayerController>();
                if (player != null)
                {
                    player.TakeDamage(30f);
                }
            }
        }
        resonanceStacks = 0;
    }

    public float GetCalculatedAttackDamage(float baseDamage)
    {
        return baseDamage * (1f + resonanceStacks * resonanceDamageBonus);
    }

    public void TakeDamage(float damage, float toughnessDamage)
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

    public void TriggerBreak()
    {
        resonanceStacks = 0;
        resonanceTimer = resonanceInterval;
        Debug.Log("【机制触发】怪物被破韧！共鸣层数已清零重置！");

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
        Debug.Log("【韧性重置】怪物韧性条已回满！");
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
        Gizmos.color = new Color(1f, 0f, 0f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, aoeRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 2.0f);
    }
}