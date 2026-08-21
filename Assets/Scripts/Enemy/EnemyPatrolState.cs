using UnityEngine;

[CreateAssetMenu(fileName = "EnemyPatrolState", menuName = "FSM/Enemy/PatrolState")]
public class EnemyPatrolState : State<EnemyController>
{
    public float patrolSpeed = 2f;
    public float patrolDistance = 3f;
    private Vector3 startPos;
    private bool movingRight = true;

    public override void OnEnter(EnemyController enemy)
    {
        if (enemy != null)
        {
            startPos = enemy.transform.position;
        }
    }

    public override void LogicUpdate(EnemyController enemy)
    {
        if (enemy == null) return;
        Transform t = enemy.transform;
        if (movingRight)
        {
            t.Translate(Vector3.right * patrolSpeed * Time.deltaTime);
            if (t.position.x >= startPos.x + patrolDistance) movingRight = false;
        }
        else
        {
            t.Translate(Vector3.left * patrolSpeed * Time.deltaTime);
            if (t.position.x <= startPos.x - patrolDistance) movingRight = true;
        }
    }

    public override void TransitionChecks(EnemyController enemy)
    {
        if (enemy == null) return;
        if (enemy.playerTransform != null)
        {
            float dist = Vector3.Distance(enemy.transform.position, enemy.playerTransform.position);
            if (dist <= 6f && enemy.chaseState != null)
            {
                enemy.stateMachine.ChangeState(enemy.chaseState);
            }
        }
    }
}