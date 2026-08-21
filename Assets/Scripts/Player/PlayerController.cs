using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour, IDamageable
{
    [Header("Stats Asset")]
    public CharacterStatsSO statsAsset;

    [Header("Runtime Attributes")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float maxToughness = 100f;
    public float currentToughness;

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
    public State deadState;

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
            maxToughness = statsAsset.maxToughness;
            moveSpeed = statsAsset.moveSpeed;
            jumpForce = statsAsset.jumpForce;
            attackDamage = statsAsset.attackDamage;
            toughnessDamage = statsAsset.toughnessDamage;
        }

        currentHealth = maxHealth;
        currentToughness = maxToughness;
    }

    private void Start()
    {
        if (stateMachine != null && stateMachine.CurrentState == null && idleState != null)
        {
            stateMachine.ChangeState(idleState);
        }
        UpdateUI();
    }

    // 接口实现：支持范围 AOE / 敌人攻击多态判定
    public void TakeDamage(float damage, float toughnessDamage = 0f)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentToughness -= toughnessDamage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        currentToughness = Mathf.Clamp(currentToughness, 0, maxToughness);
        UpdateUI();

        Debug.Log($"【玩家受击】伤害: {damage} | 剩余生命: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void HideUI()
    {
        if (healthBar != null) healthBar.gameObject.SetActive(false);
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
        if (isDead) return;

        if (stateMachine != null && deadState != null)
        {
            stateMachine.ChangeState(deadState);
        }
        else
        {
            isDead = true;
            HideUI();
            if (rb != null) { rb.linearVelocity = Vector3.zero; rb.isKinematic = true; }
            var anim = GetComponent<Animator>();
            if (anim != null) anim.SetTrigger("Die");
            if (stateMachine != null) stateMachine.enabled = false;
        }
    }
}