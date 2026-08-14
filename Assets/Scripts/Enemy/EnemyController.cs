// 【敌人模块】敌人主控制器，管理血条/韧性条 UI、受击扣血/削韧及触发破韧(Break)硬直
using UnityEngine;
using UnityEngine.UI;

public class EnemyController : MonoBehaviour
{
    [Header("属性设置")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float maxToughness = 100f;
    public float currentToughness;

    [Header("破韧增伤设置")]
    public float vulnerableDamageMultiplier = 1.25f; // 破韧状态受到的伤害倍率 (1.25倍)
    [HideInInspector] public bool isVulnerable = false; // 是否处于破韧硬直状态

    [Header("UI 槽位")]
    public Slider healthBar;
    public Slider toughnessBar;

    [Header("敌人 FSM 状态资源配置")]
    public State idleState;
    public State patrolState;
    public State chaseState;
    public State attackState;
    public State vulnerableState;

    private StateMachine stateMachine;
    private bool isDead = false;

    private void Start()
    {
        currentHealth = maxHealth;
        currentToughness = maxToughness;
        stateMachine = GetComponent<StateMachine>();

        UpdateUI();

        if (patrolState != null)
        {
            stateMachine.ChangeState(Instantiate(patrolState));
        }
    }

    public void TakeDamage(float hpDamage, float toughnessDamage)
    {
        if (isDead) return;

        // 核心改动：如果处于破韧状态，伤害提升至 1.25 倍，且期间不重复削韧
        if (isVulnerable)
        {
            hpDamage *= vulnerableDamageMultiplier;
            toughnessDamage = 0f;
            Debug.Log($"【破韧易伤触发】怪物处于破韧状态，受到 1.25 倍伤害！实际伤害: {hpDamage}");
        }

        currentHealth -= hpDamage;
        currentToughness -= toughnessDamage;

        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        currentToughness = Mathf.Clamp(currentToughness, 0, maxToughness);

        UpdateUI();

        Debug.Log($"怪物受击！生命剩余: {currentHealth}/{maxHealth} | 韧性剩余: {currentToughness}/{maxToughness}");

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        // 只有在非破韧状态下，韧性归零才触发破韧
        if (!isVulnerable && currentToughness <= 0)
        {
            TriggerBreak();
        }
    }

    private void UpdateUI()
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

    private void TriggerBreak()
    {
        if (stateMachine != null && vulnerableState != null)
        {
            stateMachine.ChangeState(Instantiate(vulnerableState));
        }
    }

    // 破韧倒计时结束后由 EnemyVulnerableState 调用，重置韧性
    public void ResetToughness()
    {
        if (isDead) return;

        isVulnerable = false;
        currentToughness = maxToughness;
        UpdateUI();

        Debug.Log("【韧性重置】怪物破韧时间结束，韧性条已回满！");

        if (stateMachine != null && chaseState != null)
        {
            stateMachine.ChangeState(Instantiate(chaseState));
        }
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("【死亡】怪物生命值归零，已被击败！");
        Destroy(gameObject, 0.2f);
    }
}