using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 自动战斗代理：已加入按键防抖冷却与多实例防冲突
/// </summary>
public class AutoCombatAgent : MonoBehaviour
{
    [Header("自动战斗开关 (可在运行中按 Z 键或在此处手动勾选)")]
    public bool isAutoEnabled = false;

    [Header("攻击与判定配置")]
    [SerializeField] private float attackRange = 2.0f;
    [SerializeField] private float attackCooldown = 0.8f;

    private float attackTimer;
    private float toggleCooldownTimer = 0f; // 按键防抖计时器
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

        // 2. 按键检测 (加入 0.25 秒防抖，杜绝连击误触)
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
                toggleCooldownTimer = 0.25f; // 锁定 0.25 秒防抖
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
        ExecuteAutoDecisionTree();
    }

    private void ExecuteAutoDecisionTree()
    {
        var resonance = EnvironmentalResonance.Instance;
        int stacks = resonance != null ? resonance.resonanceStacks : 0;
        bool isWarning = resonance != null && resonance.isWarningAOE;

        // 决策分支 1：3 层满层预警 -> 生存避险策略
        if (isWarning && resonance != null)
        {
            Vector3 center = resonance.transform.position;
            float distToCenter = Vector3.Distance(transform.position, center);
            float safeRadius = resonance.aoeRadius + 1.5f;

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

                MoveTowards(transform.position + escapeDir.normalized * 5f);
                return;
            }
        }
        else
        {
            isEvadingAOE = false;
        }

        // 决策分支 2：选择攻击目标
        EnemyController target = SelectBestTarget(stacks >= 2);
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
            float score = dist;

            if (prioritizeBreakOrKill && enemy.stats != null)
            {
                score = enemy.stats.currentToughness * 0.4f + enemy.stats.currentHealth * 0.3f + dist;
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

        if (rb != null && !rb.isKinematic)
        {
            rb.linearVelocity = Vector3.zero;
        }
    }

    private void StopMove()
    {
        if (rb != null && !rb.isKinematic)
        {
            rb.linearVelocity = Vector3.zero;
        }
    }

    private void AttackTarget(EnemyController target)
    {
        float damage = player != null ? player.attackDamage : 10.0f;
        float toughnessDamage = player != null ? player.toughnessDamage : 35.0f;

        target.TakeDamage(damage, toughnessDamage);
        Debug.Log($"【AI 攻击】对 [{target.gameObject.name}] 造成 {damage} 点伤害与 {toughnessDamage} 点削韧！");
    }
}