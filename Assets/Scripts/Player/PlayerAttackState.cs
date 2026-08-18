using UnityEngine;

[CreateAssetMenu(fileName = "PlayerAttackState", menuName = "FSM/Player/AttackState")]
public class PlayerAttackState : State
{
    public float attackDuration = 0.4f;
    public float attackRange = 2.5f;
    private float timer;

    public override void OnEnter(StateMachine stateMachine)
    {
        timer = attackDuration;
        var controller = stateMachine.GetComponent<PlayerController>();
        if (controller == null) return;

        Debug.Log("【玩家攻击】挥刀攻击！");

        // 范围判定敌人
        Collider[] hits = Physics.OverlapSphere(stateMachine.transform.position, attackRange);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                var enemy = hit.GetComponent<EnemyController>();
                if (enemy != null)
                {
                    enemy.TakeDamage(controller.attackDamage, controller.toughnessDamage);
                }
            }
        }
    }

    public override void LogicUpdate(StateMachine stateMachine)
    {
        timer -= Time.deltaTime;
    }

    public override void TransitionChecks(StateMachine stateMachine)
    {
        if (timer <= 0f)
        {
            var controller = stateMachine.GetComponent<PlayerController>();
            if (controller != null && controller.idleState != null)
            {
                stateMachine.ChangeState(controller.idleState);
            }
        }
    }
}