using UnityEngine;

// 【敌人状态】攻击状态，对玩家造成伤害并控制攻击冷却/后摇
[CreateAssetMenu(fileName = "EnemyAttackState", menuName = "FSM/Enemy/AttackState")]
public class EnemyAttackState : State
{
    private float attackTimer;
    public float attackDamage = 15f; // 敌人每次攻击伤害

    public override void OnEnter(StateMachine stateMachine)
    {
        attackTimer = 1.2f; // 攻击动画/攻击后摇持续 1.2 秒
        Debug.Log("【FSM驱动】敌人向玩家发起攻击！");

        // 尝试寻找玩家并造成伤害
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            float dist = Vector3.Distance(stateMachine.transform.position, playerObj.transform.position);
            if (dist <= 2.2f)
            {
                PlayerController player = playerObj.GetComponent<PlayerController>();
                if (player != null)
                {
                    player.TakeDamage(attackDamage);
                }
            }
        }
    }

    public override void LogicUpdate(StateMachine stateMachine)
    {
        attackTimer -= Time.deltaTime;
    }

    public override void TransitionChecks(StateMachine stateMachine)
    {
        if (attackTimer <= 0)
        {
            var enemy = stateMachine.GetComponent<EnemyController>();
            if (enemy != null && enemy.chaseState != null)
            {
                enemy.GetComponent<StateMachine>().ChangeState(Instantiate(enemy.chaseState));
            }
        }
    }
}