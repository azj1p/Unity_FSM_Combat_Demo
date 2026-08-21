using UnityEngine;

[CreateAssetMenu(fileName = "EnemyChaseState", menuName = "FSM/Enemy/ChaseState")]
public class EnemyChaseState : State<EnemyController>
{
    public float chaseSpeed = 3.5f;
    public float attackRange = 1.5f;

    public override void LogicUpdate(EnemyController enemy)
    {
        if (enemy == null || enemy.playerTransform == null) return;

        Vector3 targetPos = enemy.playerTransform.position;
        targetPos.y = enemy.transform.position.y;
        enemy.transform.LookAt(targetPos);

        if (enemy.rb != null)
        {
            Vector3 nextPos = Vector3.MoveTowards(
                enemy.rb.position,
                targetPos,
                chaseSpeed * Time.deltaTime
            );
            enemy.rb.MovePosition(nextPos);
        }
        else
        {
            enemy.transform.position = Vector3.MoveTowards(
                enemy.transform.position,
                targetPos,
                chaseSpeed * Time.deltaTime
            );
        }
    }

    public override void TransitionChecks(EnemyController enemy)
    {
        if (enemy == null || enemy.playerTransform == null) return;

        float distance = Vector3.Distance(enemy.transform.position, enemy.playerTransform.position);
        if (distance <= attackRange && enemy.attackState != null)
        {
            enemy.stateMachine.ChangeState(enemy.attackState);
        }
    }
}