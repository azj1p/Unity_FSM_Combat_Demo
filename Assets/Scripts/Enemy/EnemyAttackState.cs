using UnityEngine;

[CreateAssetMenu(fileName = "EnemyAttackState", menuName = "FSM/Enemy/AttackState")]
public class EnemyAttackState : State
{
    [Header("Attack Settings")]
    public float attackDuration = 1.0f;     // 攻击动作与后摇总时长
    public float baseAttackDamage = 15.0f;  // 基础攻击伤害
    public float attackRange = 2.0f;        // 攻击有效判定距离

    public override void OnEnter(StateMachine stateMachine)
    {
        var controller = stateMachine.GetComponent<EnemyController>();
        if (controller == null) return;

        // 设置当前怪物专属的攻击计时器（避免 SO 共享变量冲突）
        controller.attackTimer = attackDuration;

        // 攻击时清空水平残余物理速度，防止滑步
        var rb = stateMachine.GetComponent<Rigidbody>();
        if (rb != null && !rb.isKinematic)
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }

        // 面朝玩家
        if (controller.playerTransform != null)
        {
            Vector3 lookTarget = controller.playerTransform.position;
            lookTarget.y = stateMachine.transform.position.y;
            stateMachine.transform.LookAt(lookTarget);
        }

        // 触发攻击动画
        var animator = stateMachine.GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        // 动态计算共鸣增伤并进行多态伤害结算
        float finalDamage = controller.GetCalculatedAttackDamage(baseAttackDamage);
        if (controller.playerTransform != null)
        {
            float distance = Vector3.Distance(stateMachine.transform.position, controller.playerTransform.position);
            if (distance <= attackRange)
            {
                var damageable = controller.playerTransform.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(finalDamage);
                    Debug.Log($"【敌人攻击】命中玩家！造成 {finalDamage} 点伤害 (共鸣加成: {controller.resonanceStacks} 层)");
                }
            }
        }
    }

    public override void LogicUpdate(StateMachine stateMachine)
    {
        var controller = stateMachine.GetComponent<EnemyController>();
        if (controller != null)
        {
            controller.attackTimer -= Time.deltaTime;
        }
    }

    public override void TransitionChecks(StateMachine stateMachine)
    {
        var controller = stateMachine.GetComponent<EnemyController>();
        if (controller == null || controller.playerTransform == null) return;

        // 攻击后摇结束，进行状态闭环流转判定（防止发呆死锁）
        if (controller.attackTimer <= 0f)
        {
            float distance = Vector3.Distance(stateMachine.transform.position, controller.playerTransform.position);

            // 玩家仍在攻击范围内 -> 连续攻击循环
            if (distance <= attackRange && controller.attackState != null)
            {
                stateMachine.ChangeState(controller.attackState);
            }
            // 玩家跑远 -> 切入追击状态
            else if (controller.chaseState != null)
            {
                stateMachine.ChangeState(controller.chaseState);
            }
            // 默认兜底切回待机
            else if (controller.idleState != null)
            {
                stateMachine.ChangeState(controller.idleState);
            }
        }
    }
}