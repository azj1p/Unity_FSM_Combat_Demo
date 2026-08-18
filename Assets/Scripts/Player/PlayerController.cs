using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour, IDamageable
{
    [Header("Stats Asset (数据驱动/可选)")]
    public CharacterStatsSO statsAsset;

    [Header("Runtime Attributes")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float moveSpeed = 5.0f;
    public float jumpForce = 5.0f;
    public float attackDamage = 20.0f;
    public float toughnessDamage = 25.0f;

    [Header("UI")]
    public Slider healthBar;

    [Header("States")]
    public State idleState;
    public State moveState;
    public State attackState;
    public State deadState; // 可选：若配置了玩家死亡状态

    [HideInInspector] public StateMachine stateMachine;
    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public bool isDead;

    private void Awake()
    {
        stateMachine = GetComponent<StateMachine>();
        rb = GetComponent<Rigidbody>();

        if (statsAsset != null)
        {
            maxHealth = statsAsset.maxHealth;
            moveSpeed = statsAsset.moveSpeed;
            jumpForce = statsAsset.jumpForce;
            attackDamage = statsAsset.attackDamage;
            toughnessDamage = statsAsset.toughnessDamage;
        }

        currentHealth = maxHealth;
    }

    private void Start()
    {
        if (stateMachine != null && stateMachine.CurrentState == null && idleState != null)
        {
            stateMachine.ChangeState(idleState);
        }
        UpdateUI();
    }

    public void TakeDamage(float damage, float toughnessDamage = 0f)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateUI();

        Debug.Log($"【玩家受击】受到 {damage} 点伤害！剩余血量: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
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

    public void HideUI()
    {
        if (healthBar != null)
        {
            healthBar.gameObject.SetActive(false);
        }
    }

    public void Die()
    {
        if (isDead) return;

        // 由 FSM 驱动进入死亡状态
        if (stateMachine != null && deadState != null)
        {
            stateMachine.ChangeState(deadState);
        }
        else
        {
            // 兜底逻辑
            isDead = true;
            HideUI();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.isKinematic = true;
            }
            var animator = GetComponent<Animator>();
            if (animator != null) animator.SetTrigger("Die");
            if (stateMachine != null) stateMachine.enabled = false;
        }
    }
}