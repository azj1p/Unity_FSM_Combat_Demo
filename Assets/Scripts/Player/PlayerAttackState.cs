//using UnityEngine;

//// 【玩家状态】攻击状态，处理玩家攻击判定与后摇/状态切回
//[CreateAssetMenu(fileName = "PlayerAttackState", menuName = "FSM/Player/AttackState")]
//public class PlayerAttackState : State
//{
//    public override void OnEnter(StateMachine stateMachine)
//    {
//        var player = stateMachine.GetComponent<PlayerController>();
//        if (player == null) return;

//        Debug.Log("【FSM驱动】玩家发起攻击！");

//        Collider[] hitColliders = Physics.OverlapSphere(player.transform.position, 2f);
//        foreach (var hit in hitColliders)
//        {
//            if (hit.CompareTag("Enemy"))
//            {
//                EnemyController enemy = hit.GetComponent<EnemyController>();
//                if (enemy != null)
//                {
//                    enemy.TakeDamage(player.attackDamage, player.toughnessDamage);
//                }
//            }
//        }
//    }

//    public override void TransitionChecks(StateMachine stateMachine)
//    {
//        var player = stateMachine.GetComponent<PlayerController>();
//        if (player != null)
//        {
//            player.ChangeState(player.idleState);
//        }
//    }
//}

// 【玩家状态】攻击状态，处理攻击伤害判定、后摇与状态恢复
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerAttackState", menuName = "FSM/Player/AttackState")]
public class PlayerAttackState : State
{
    [Header("攻击设置")]
    public float attackDuration = 0.6f; // 攻击后摇持续时间（在此期间无法移动）
    public float attackRange = 3.5f;    // 攻击判定范围（从 2.5 调大到 3.5，防止挥空）

    private float timer;

    public override void OnEnter(StateMachine stateMachine)
    {
        timer = attackDuration;

        var player = stateMachine.GetComponent<PlayerController>();
        if (player == null) return;

        // 使用物理球形检测：以玩家前方 1 米为中心，检索 attackRange 范围内的所有物体
        Vector3 attackCenter = player.transform.position + player.transform.forward * 1.0f;
        Collider[] hitColliders = Physics.OverlapSphere(attackCenter, attackRange);

        bool hitAnyEnemy = false;

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Enemy"))
            {
                EnemyController enemy = hitCollider.GetComponent<EnemyController>();
                if (enemy != null)
                {
                    enemy.TakeDamage(player.attackDamage, player.toughnessDamage);
                    hitAnyEnemy = true;
                }
            }
        }

        if (!hitAnyEnemy)
        {
            Debug.Log("【玩家攻击】未击中目标（未面向敌人或距离过远）");
        }
    }

    public override void LogicUpdate(StateMachine stateMachine)
    {
        timer -= Time.deltaTime;
    }

    public override void TransitionChecks(StateMachine stateMachine)
    {
        // 攻击后摇结束后，自动切回待机状态，恢复移动能力
        if (timer <= 0)
        {
            var player = stateMachine.GetComponent<PlayerController>();
            if (player != null)
            {
                player.ChangeState(player.idleState);
            }
        }
    }
}