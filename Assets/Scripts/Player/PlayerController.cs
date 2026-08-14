using UnityEngine;
using UnityEngine.UI;

// 【玩家模块】玩家主控制器，管理生命值(HP)、受击/死亡逻辑及 FSM 状态切换入口
public class PlayerController : MonoBehaviour
{
    [Header("生命值属性")]
    public float maxHealth = 100f;
    public float currentHealth;
    public Slider healthBar; // 玩家血条 UI

    [Header("移动属性")]
    public float moveSpeed = 5f;

    [Header("战斗属性")]
    public float attackDamage = 25f;
    public float toughnessDamage = 35f;

    [Header("玩家 FSM 状态配置")]
    public State idleState;
    public State moveState;
    public State attackState;

    private StateMachine stateMachine;
    private bool isDead = false;

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateUI();

        stateMachine = GetComponent<StateMachine>();
        if (stateMachine == null)
        {
            stateMachine = gameObject.AddComponent<StateMachine>();
        }

        if (idleState != null)
        {
            stateMachine.ChangeState(Instantiate(idleState));
        }
    }

    // 玩家受伤逻辑
    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateUI();

        Debug.Log($"玩家遭到敌人攻击！当前生命值剩余: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void UpdateUI()
    {
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("【玩家死亡】玩家生命值归零！");
        Destroy(gameObject, 0.2f);
    }

    public void ChangeState(State newState)
    {
        if (stateMachine != null && newState != null)
        {
            stateMachine.ChangeState(Instantiate(newState));
        }
    }
}