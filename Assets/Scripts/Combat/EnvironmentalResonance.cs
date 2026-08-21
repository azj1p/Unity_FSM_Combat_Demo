using System;
using UnityEngine;

// 敌人阶级定义：决定破韧与击杀时对环境共鸣层数的影响
public enum EnemyRank
{
    Normal, // 普通怪：破韧/击杀扣减 1 层
    Elite,  // 精英怪：破韧/击杀层数清零
    Boss    // 首领怪：破韧/击杀层数清零 + 行动条推条 25%
}

public class EnvironmentalResonance : MonoBehaviour
{
    public static EnvironmentalResonance Instance { get; private set; }

    [Header("Resonance Configuration (共鸣数值规则)")]
    [Tooltip("当前共鸣层数")]
    public int resonanceStacks = 0;
    [Tooltip("最大共鸣层数")]
    public int maxResonanceStacks = 3;
    [Tooltip("每层共鸣提供的敌人伤害加成（默认每层 8%）")]
    public float resonanceDamageBonus = 0.08f;
    [Tooltip("基础行动周期/层数积累间隔（秒）")]
    public float resonanceInterval = 6.0f;
    [Tooltip("满层 AOE 爆发范围半径（米）")]
    public float aoeRadius = 8.0f;
    [Tooltip("满层 AOE 爆发伤害")]
    public float aoeDamage = 30.0f;

    [Header("Action Value System (行动值/倒计时系统)")]
    [Tooltip("当前行动条倒计时/行动值剩余时间")]
    public float currentTimer;
    [Tooltip("全场敌人速度对行动周期的影响倍率")]
    public float globalSpeedMultiplier = 1.0f;

    [Header("AOE Warning (预警机制)")]
    [Tooltip("AOE 爆发前的预警蓄力时间（秒）")]
    public float aoeWarningDuration = 1.5f;
    [HideInInspector] public bool isWarningAOE = false;
    [HideInInspector] public float warningTimer = 0f;

    // 事件系统：用于驱动 UI 刷新与视觉反馈（解耦 UI 与核心逻辑）
    public event Action<int, int> OnResonanceStacksChanged; // (当前层数, 最大层数)
    public event Action<float, float> OnTimerUpdated;       // (当前剩余时间, 总时间)
    public event Action<bool, float> OnAOEWarningState;     // (是否处于预警, 预警倒计时)
    public event Action OnAOETriggered;                    // AOE 爆发触发事件

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        currentTimer = resonanceInterval;
    }

    private void Start()
    {
        OnResonanceStacksChanged?.Invoke(resonanceStacks, maxResonanceStacks);
        OnTimerUpdated?.Invoke(currentTimer, resonanceInterval);
    }

    private void Update()
    {
        // 预警状态下的独立倒计时
        if (isWarningAOE)
        {
            warningTimer -= Time.deltaTime;
            OnAOEWarningState?.Invoke(true, warningTimer);

            if (warningTimer <= 0f)
            {
                ExecuteAOEExplosion();
            }
            return;
        }

        // 行动值系统倒计时（受全局速度修饰）
        if (currentTimer > 0f)
        {
            currentTimer -= Time.deltaTime * globalSpeedMultiplier;
            OnTimerUpdated?.Invoke(Mathf.Max(currentTimer, 0f), resonanceInterval);

            if (currentTimer <= 0f)
            {
                AccumulateResonance();
            }
        }
    }

    // 积累共鸣层数逻辑
    private void AccumulateResonance()
    {
        if (resonanceStacks < maxResonanceStacks)
        {
            resonanceStacks++;
            Debug.Log($"【环境共鸣】行动周期结束，层数累加至: {resonanceStacks}/{maxResonanceStacks} (+{resonanceStacks * resonanceDamageBonus * 100}% 增伤)");
            OnResonanceStacksChanged?.Invoke(resonanceStacks, maxResonanceStacks);

            if (resonanceStacks >= maxResonanceStacks)
            {
                StartAOEWarning();
                return;
            }
        }

        // 重置倒计时进入下一行动周期
        currentTimer = resonanceInterval;
    }

    // 进入满层 AOE 预警阶段（提供玩家闪避/应对窗口）
    private void StartAOEWarning()
    {
        isWarningAOE = true;
        warningTimer = aoeWarningDuration;
        Debug.LogWarning($"【环境威胁】共鸣满层！进入 {aoeWarningDuration} 秒 AOE 爆发预警！");
        OnAOEWarningState?.Invoke(true, warningTimer);
    }

    // 执行 AOE 伤害爆发并结算我方受击
    private void ExecuteAOEExplosion()
    {
        isWarningAOE = false;
        OnAOEWarningState?.Invoke(false, 0f);
        OnAOETriggered?.Invoke();

        Debug.LogError("【环境共鸣爆发】AOE 爆发释放！");

        // 统一在环境层遍历伤害（仅对玩家阵营结算）
        Collider[] hits = Physics.OverlapSphere(transform.position, aoeRadius);
        foreach (var hit in hits)
        {
            // 策划案规则：AOE 仅命中玩家（我方全体），不对敌人造成误伤
            if (hit.CompareTag("Player") || hit.GetComponent<PlayerController>() != null)
            {
                var damageable = hit.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(aoeDamage, 0f);
                    Debug.Log($"【环境共鸣爆发】成功命中玩家！造成 {aoeDamage} 点环境伤害。");
                }
            }
        }

        // 策划案对齐：AOE 结束后共鸣层数重置为 1 层继续积累
        resonanceStacks = 1;
        currentTimer = resonanceInterval;
        OnResonanceStacksChanged?.Invoke(resonanceStacks, maxResonanceStacks);
        OnTimerUpdated?.Invoke(currentTimer, resonanceInterval);
    }

    // 敌人破韧/击杀时的环境联动响应
    public void OnEnemyBrokenOrKilled(EnemyRank rank, bool isKilled)
    {
        switch (rank)
        {
            case EnemyRank.Normal:
                resonanceStacks = Mathf.Max(0, resonanceStacks - 1);
                Debug.Log($"【环境反制】普通怪被{(isKilled ? "击杀" : "破韧")}，共鸣层数 -1，当前: {resonanceStacks}");
                break;

            case EnemyRank.Elite:
                resonanceStacks = 0;
                Debug.Log($"【环境反制】精英怪被{(isKilled ? "击杀" : "破韧")}，共鸣层数清零！");
                break;

            case EnemyRank.Boss:
                resonanceStacks = 0;
                // Boss 破韧/击杀：推条 25% 行动值（延长倒计时）
                currentTimer += resonanceInterval * 0.25f;
                Debug.Log($"【环境反制】Boss 被{(isKilled ? "击杀" : "破韧")}，共鸣归零且推条 25%！当前倒计时: {currentTimer:F1}s");
                break;
        }

        // 如果在预警阶段成功打断破韧，直接取消预警
        if (isWarningAOE && resonanceStacks < maxResonanceStacks)
        {
            isWarningAOE = false;
            OnAOEWarningState?.Invoke(false, 0f);
            Debug.Log("【环境反制】在预警期间成功反制，AOE 爆发被打断！");
        }

        OnResonanceStacksChanged?.Invoke(resonanceStacks, maxResonanceStacks);
    }

    // 提供给外部的统一增伤倍率查询接口
    public float GetDamageBonusMultiplier()
    {
        return 1.0f + (resonanceStacks * resonanceDamageBonus);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = isWarningAOE ? new Color(1f, 0f, 0f, 0.4f) : new Color(1f, 0.8f, 0f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, aoeRadius);
    }
}