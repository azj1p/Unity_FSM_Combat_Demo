using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 自动战斗代理：已接入 FSM 攻击流转、0.3s 索敌目标缓存与重力垂直速度保护
/// </summary>
public class AutoCombatAgent : MonoBehaviour
{
    [Header("自动战斗开关 (可在运行中按 Z 键或在此处手动勾选)")]
    public bool isAutoEnabled = false;

    [Header("攻击与判定配置")]
    [SerializeField] private float attackRange = 2.0f;
    [SerializeField] private float attackCooldown = 0.8f;

    [Header("AI 避险与机动配置 (消除魔法数字)")]
    [SerializeField] private float evadeBufferDistance = 1.5f; // 满层 AOE 避险缓冲距离
    [SerializeField] private float evadeMoveDistance = 5.0f;   // 逃跑目标位移距离
    [SerializeField] private float toggleCooldown = 0.25f;     // Z 键防抖冷却时间

    [Header("AI 索敌与 GC 优化配置")]
    [SerializeField] private float retargetInterval = 0.3f;    // 目标重选间隔 (避免每帧全量遍历分配 GC)
    [SerializeField] private float distanceWeight = 0.9f;      // 距离权重
    [SerializeField] private float toughnessWeight = 0.4f;     // 临界状态韧性权重
    [SerializeField] private float healthWeight = 0.3f;        // 临界状态血量权重

    [Header("调试日志")]
    [SerializeField] private bool showDebugLog = false;        // 关闭高频攻击日志，降低控制台噪音

    private float attackTimer;
    private float retargetTimer = 0f;       // 索敌缓存刷新计时器
    private float toggleCooldownTimer = 0f; // 按键防抖计时器
    private EnemyController cachedTarget;   // 目标缓存引用
    private PlayerController player;
    private Rigidbody rb;
    private bool isEvadingAOE = false;

    private void Awake()
    {
        player = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        // 1. 防抖计时递减
        if (toggleCooldownTimer > 0f)
        {
            toggleCooldownTimer -= Time.unscaledDeltaTime;
        }

        // 2. 按键检测 (加入防抖冷却，杜绝连击误触)
        if (toggleCooldownTimer <= 0f)
        {
            bool zPressed = false;
            if (Keyboard.current != null && Keyboard.current.zKey.wasPressedThisFrame)
            {
                zPressed = true;
            }
            else if (Input.GetKeyDown(KeyCode.Z))
            {
                zPressed = true;
            }

            if (zPressed)
            {
                toggleCooldownTimer = toggleCooldown;
                isAutoEnabled = !isAutoEnabled;
                StopMove();

                if (isAutoEnabled)
                {
                    Debug.Log("<color=#00FF00>【自动战斗】已开启 (AI 正式接管控制)</color>");
                }
                else
                {
                    Debug.Log("<color=#FF4444>【自动战斗】已关闭 (已交还玩家手动控制)</color>");
                }
            }
        }

        // 3. 未开启时彻底交出控制权
        if (!isAutoEnabled) return;

        // 4. 死亡保护
        if (player != null && player.isDead)
        {
            StopMove();
            return;
        }

        attackTimer -= Time.deltaTime;
        retargetTimer -= Time.deltaTime;

        ExecuteAutoDecisionTree();
    }

    private void ExecuteAutoDecisionTree()
    {
        // 若当前处于攻击动作硬直中，暂停移动指令，遵循 FSM 攻击窗口
        if (player != null && player.stateMachine != null && player.attackState != null)
        {
            if (player.stateMachine.CurrentState == player.attackState)
            {
                StopMove();
                return;
            }
        }

        var resonance = EnvironmentalResonance.Instance;
        int stacks = resonance != null ? resonance.resonanceStacks : 0;
        bool isWarning = resonance != null && resonance.isWarningAOE;

        // 决策分支 1：3 层满层预警 -> 生存避险策略
        if (isWarning && resonance != null)
        {
            Vector3 center = resonance.transform.position;
            float distToCenter = Vector3.Distance(transform.position, center);
            float safeRadius = resonance.aoeRadius + evadeBufferDistance;

            if (distToCenter < safeRadius)
            {
                isEvadingAOE = true;
            }
            else if (distToCenter >= safeRadius + 1.0f)
            {
                isEvadingAOE = false;
            }

            if (isEvadingAOE)
            {
                Vector3 escapeDir = (transform.position - center);
                escapeDir.y = 0;
                if (escapeDir.sqrMagnitude < 0.001f) escapeDir = Vector3.forward;

                MoveTowards(transform.position + escapeDir.normalized * evadeMoveDistance);
                return;
            }
        }
        else
        {
            isEvadingAOE = false;
        }

        // 决策分支 2：选择攻击目标 (带 0.3s 缓存，避免每帧执行 FindObjectsByType 导致 GC 分配)
        if (cachedTarget == null || cachedTarget.isDead || retargetTimer <= 0f)
        {
            cachedTarget = SelectBestTarget(stacks >= 2);
            retargetTimer = retargetInterval;
        }

        EnemyController target = cachedTarget;
        if (target == null)
        {
            StopMove();
            return;
        }

        float dist = Vector3.Distance(transform.position, target.transform.position);

        if (dist > attackRange)
        {
            MoveTowards(target.transform.position);
        }
        else
        {
            StopMove();

            Vector3 lookTarget = target.transform.position;
            lookTarget.y = transform.position.y;
            Vector3 lookDir = lookTarget - transform.position;
            if (lookDir.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(lookDir);
            }

            if (attackTimer <= 0f)
            {
                AttackTarget(target);
                attackTimer = attackCooldown;
            }
        }
    }

    private EnemyController SelectBestTarget(bool prioritizeBreakOrKill)
    {
        EnemyController[] enemies = FindObjectsByType<EnemyController>();
        EnemyController best = null;
        float minScore = float.MaxValue;

        foreach (var enemy in enemies)
        {
            if (enemy == null || enemy.isDead) continue;

            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            float score = dist * distanceWeight;

            if (prioritizeBreakOrKill && enemy.stats != null)
            {
                score += enemy.stats.currentToughness * toughnessWeight + enemy.stats.currentHealth * healthWeight;
            }

            if (score < minScore)
            {
                minScore = score;
                best = enemy;
            }
        }
        return best;
    }

    private void MoveTowards(Vector3 targetPos)
    {
        Vector3 dir = (targetPos - transform.position);
        dir.y = 0;
        if (dir.sqrMagnitude < 0.001f) return;

        dir.Normalize();
        float speed = player != null ? player.moveSpeed : 5.0f;

        transform.position += dir * speed * Time.deltaTime;
        transform.forward = dir;

        // 关键修复：仅消除水平速度惯性，保留 Y 轴物理垂直速度，允许重力正常拉回地面
        if (rb != null && !rb.isKinematic)
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        }
    }

    private void StopMove()
    {
        // 关键修复：停止移动时同样保留 Y 轴垂直速度，避免空中刹车导致悬浮停滞
        if (rb != null && !rb.isKinematic)
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        }
    }

    private void AttackTarget(EnemyController target)
    {
        // 优先驱动 PlayerController 状态机进入 PlayerAttackState，确保动作窗口与手动输入单点收敛
        if (player != null && player.stateMachine != null && player.attackState != null)
        {
            if (player.stateMachine.CurrentState != player.attackState)
            {
                player.stateMachine.ChangeState(player.attackState);
            }
        }
        else
        {
            // 兜底直接结算判定
            float damage = player != null ? player.attackDamage : 10.0f;
            float toughnessDamage = player != null ? player.toughnessDamage : 35.0f;
            target.TakeDamage(damage, toughnessDamage);
        }

        if (showDebugLog)
        {
            Debug.Log($"【AI 攻击】驱动攻击状态，锁定目标: [{target.gameObject.name}]");
        }
    }
}