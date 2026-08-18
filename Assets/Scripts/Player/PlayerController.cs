using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour, IDamageable
{
    [Header("Stats Asset (数据驱动)")]
    [Tooltip("可选：拖入配置好的 Stats 资产文件；若为空则使用下方默认值")]
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

    [HideInInspector] public StateMachine stateMachine;
    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public bool isDead;

    private void Awake()
    {
        // 缓存组件，消除运行期 GetComponent 开销
        stateMachine = GetComponent<StateMachine>();
        rb = GetComponent<Rigidbody>();

        // 数据驱动：从 SO 加载数值
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

        Debug.Log($"玩家受击！生命剩余: {currentHealth}/{maxHealth}");
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

    public void Die()
    {
        isDead = true;
        Debug.Log("【玩家死亡】游戏结束！");
    }
}