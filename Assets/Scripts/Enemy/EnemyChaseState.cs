using UnityEngine;

// 【敌人状态】追击状态，实时追踪玩家位置，进入攻击距离后切入攻击
[CreateAssetMenu(fileName = "EnemyChaseState", menuName = "FSM/Enemy/ChaseState")]
public class EnemyChaseState : State
{
    public float chaseSpeed = 3.5f;

    public override void LogicUpdate(StateMachine stateMachine)
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            Vector3 dir = (player.transform.position - stateMachine.transform.position).normalized;
            dir.y = 0;
            stateMachine.transform.Translate(dir * chaseSpeed * Time.deltaTime, Space.World);
        }
    }

    public override void TransitionChecks(StateMachine stateMachine)
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return;

        float dist = Vector3.Distance(stateMachine.transform.position, player.transform.position);

        if (dist <= 1.8f)
        {
            var enemy = stateMachine.GetComponent<EnemyController>();
            if (enemy != null && enemy.attackState != null)
            {
                enemy.GetComponent<StateMachine>().ChangeState(Instantiate(enemy.attackState));
            }
        }
        else if (dist > 8f)
        {
            var enemy = stateMachine.GetComponent<EnemyController>();
            if (enemy != null && enemy.patrolState != null)
            {
                enemy.GetComponent<StateMachine>().ChangeState(Instantiate(enemy.patrolState));
            }
        }
    }
}