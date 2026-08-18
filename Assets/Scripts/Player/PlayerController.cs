using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth;
    public Slider healthBar;

    [Header("Combat")]
    public float moveSpeed = 5.0f;
    public float jumpForce = 5.0f;
    public float attackDamage = 20.0f;
    public float toughnessDamage = 25.0f;

    [Header("States")]
    public State idleState;
    public State moveState;
    public State attackState;

    [HideInInspector] public StateMachine stateMachine;
    [HideInInspector] public bool isDead;

    private void Start()
    {
        currentHealth = maxHealth;
        stateMachine = GetComponent<StateMachine>();

        // 状态机兜底初始化
        if (stateMachine != null)
        {
            if (stateMachine.CurrentState == null && idleState != null)
            {
                stateMachine.ChangeState(idleState);
            }
        }

        UpdateUI();
    }

    public void TakeDamage(float damage, float toughnessDamage = 0f)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateUI();

        Debug.Log($"玩家受击！生命剩余: {currentHealth}/{maxHealth}");
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void ChangeState(State newState)
    {
        if (stateMachine != null && newState != null)
        {
            stateMachine.ChangeState(newState);
        }
    }

    public void UpdateUI()
    {
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }
    }

    public void Die()
    {
        isDead = true;
        Debug.Log("【玩家死亡】玩家生命值归零！");
    }
}