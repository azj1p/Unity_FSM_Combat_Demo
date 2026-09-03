using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerController : MonoBehaviour, IDamageable, IActionValueEntity
{
    [Header("Stats Asset")]
    public CharacterStatsSO statsAsset;

    [Header("Runtime Attributes")]
    public float maxHealth = 100f;
    public float currentHealth;

    public float moveSpeed = 5.0f;
    public float jumpForce = 5.0f;
    public float attackDamage = 10.0f;
    public float toughnessDamage = 35.0f;

    [Header("行动值系统配置 (P3-2)")]
    [SerializeField] private float actionSpeed = 20f; // 玩家基准行动速度 (100 / 20 = 5秒蓄满一轮)

    [Header("UI")]
    public Slider healthBar;
    public TMP_Text healthText;

    [Header("States")]
    public State idleState;
    public State moveState;
    public State attackState;
    public State deadState;

    [HideInInspector] public StateMachine stateMachine;
    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public bool isDead;
    [HideInInspector] public bool isStaggered; // 受击硬直状态标识

    private float staggerTimer = 0f;

    // --- IActionValueEntity 接口实现 ---
    public float ActionSpeed => actionSpeed;

    public void ExecuteAction()
    {
        if (isDead) return;
        // 玩家行动值蓄满时的回调：
        // 动作制战斗由实时操作主导，此处作为被动节拍或预留机制接口
    }
    // ----------------------------------

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

    private void OnEnable()
    {
        // 激活时注册进全局行动序列
        if (ActionValueSystem.Instance != null)
        {
            ActionValueSystem.Instance.RegisterEntity(this);
        }
    }

    private void OnDisable()
    {
        // 失活时安全注销
        if (ActionValueSystem.Instance != null)
        {
            ActionValueSystem.Instance.UnregisterEntity(this);
        }
    }

    private void Start()
    {
        if (stateMachine != null && stateMachine.CurrentState == null && idleState != null)
        {
            stateMachine.ChangeState(idleState);
        }

        // 弥补场景加载首帧单例初始化时序可能带来的漏注
        if (ActionValueSystem.Instance != null)
        {
            ActionValueSystem.Instance.RegisterEntity(this);
        }

        UpdateUI();
    }

    private void Update()
    {
        // 硬直倒计时控制
        if (isStaggered)
        {
            staggerTimer -= Time.deltaTime;
            if (staggerTimer <= 0f)
            {
                isStaggered = false;
            }
        }
    }

    private void OnDestroy()
    {
        if (ActionValueSystem.Instance != null)
        {
            ActionValueSystem.Instance.UnregisterEntity(this);
        }
    }

    /// <summary>
    /// 受到伤害（保留接口签名，移除死代码韧性扣减）
    /// </summary>
    public void TakeDamage(float damage, float toughnessDamage = 0f)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateUI();

        Debug.Log($"【玩家受击】伤害: {damage} | 剩余生命: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 施加受击硬直（等价映射需求文档中 AOE 导致的“行动延后 10%”）
    /// </summary>
    public void ApplyStagger(float duration = 0.5f)
    {
        if (isDead) return;

        isStaggered = true;
        staggerTimer = duration;

        // 停止当前速度并打断出招
        if (rb != null && !rb.isKinematic)
        {
            rb.linearVelocity = Vector3.zero;
        }

        // 若处于攻击状态，强行退回待机状态
        if (stateMachine != null && stateMachine.CurrentState == attackState && idleState != null)
        {
            stateMachine.ChangeState(idleState);
        }

        Debug.Log($"<color=yellow>【玩家硬直】受到冲击打断，行动受限 {duration} 秒！</color>");
    }

    public void HideUI()
    {
        if (healthBar != null) healthBar.gameObject.SetActive(false);
        if (healthText != null) healthText.gameObject.SetActive(false);
    }

    public void UpdateUI()
    {
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }

        if (healthText != null)
        {
            healthText.text = $"{Mathf.CeilToInt(currentHealth)} / {Mathf.CeilToInt(maxHealth)}";
        }
    }

    public void Die()
    {
        if (isDead) return;

        // 阵亡立即移出行动序列
        if (ActionValueSystem.Instance != null)
        {
            ActionValueSystem.Instance.UnregisterEntity(this);
        }

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