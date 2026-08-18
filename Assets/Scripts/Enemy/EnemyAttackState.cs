using UnityEngine;

[CreateAssetMenu(fileName = "EnemyAttackState", menuName = "FSM/Enemy/AttackState")]
public class EnemyAttackState : State
{
    [Tooltip("攻击间隔/后摇时间（秒）")]
    public float attackTimer = 1.2f;
    [Tooltip("基础攻击力")]
    public float attackDamage = 15f;
    [Tooltip("攻击判定距离")]
    public float attackRange = 2.0f;

    private float timer;

    public override void OnEnter(StateMachine stateMachine)
    {
        timer = attackTimer;
        ExecuteAttack(stateMachine);
    }

    public override void LogicUpdate(StateMachine stateMachine)
    {
        timer -= Time.deltaTime;
    }

    public override void TransitionChecks(StateMachine stateMachine)
    {
        // 攻击后摇/冷却结束
        if (timer <= 0f)
        {
            var controller = stateMachine.GetComponent<EnemyController>();
            if (controller == null || controller.playerTransform == null) return;

            float distance = Vector3.Distance(
                stateMachine.transform.position,
                controller.playerTransform.position
            );

            if (distance > attackRange)
            {
                // 1. 玩家拉开了距离 -> 切换回追击状态
                if (controller.chaseState != null)
                {
                    stateMachine.ChangeState(controller.chaseState);
                }
            }
            else
            {
                // 2. 玩家仍留在攻击范围内 -> 重置计时器并再次发动攻击（解决原地发呆问题）
                timer = attackTimer;
                ExecuteAttack(stateMachine);
            }
        }
    }

    // 抽离独立的攻击结算逻辑
    private void ExecuteAttack(StateMachine stateMachine)
    {
        var controller = stateMachine.GetComponent<EnemyController>();
        if (controller == null || controller.playerTransform == null) return;

        // 面向玩家
        Vector3 targetPos = controller.playerTransform.position;
        targetPos.y = stateMachine.transform.position.y;
        stateMachine.transform.LookAt(targetPos);

        // 结合共鸣层数计算伤害
        float finalDamage = controller.GetCalculatedAttackDamage(attackDamage);
        Debug.Log($"【FSM驱动】敌人向玩家发起攻击！造成伤害: {finalDamage:F1} (基础 {attackDamage})");

        // 尝试触发攻击动画
        var animator = stateMachine.GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        // 判定伤害
        float distance = Vector3.Distance(stateMachine.transform.position, controller.playerTransform.position);
        if (distance <= attackRange)
        {
            var player = controller.playerTransform.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(finalDamage);
            }
        }
    }
}