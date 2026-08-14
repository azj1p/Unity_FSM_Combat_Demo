using UnityEngine;

// 【敌人状态】巡逻状态，控制敌人在设定范围内左右来回巡逻，发现玩家时切入追击
[CreateAssetMenu(fileName = "EnemyPatrolState", menuName = "FSM/Enemy/PatrolState")]
public class EnemyPatrolState : State
{
    public float patrolSpeed = 2f;
    public float patrolDistance = 3f;
    private Vector3 startPos;
    private bool movingRight = true;

    public override void OnEnter(StateMachine stateMachine)
    {
        startPos = stateMachine.transform.position;
    }

    public override void LogicUpdate(StateMachine stateMachine)
    {
        Transform t = stateMachine.transform;
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

    public override void TransitionChecks(StateMachine stateMachine)
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            float dist = Vector3.Distance(stateMachine.transform.position, player.transform.position);
            if (dist <= 6f)
            {
                var enemy = stateMachine.GetComponent<EnemyController>();
                if (enemy != null && enemy.chaseState != null)
                {
                    enemy.GetComponent<StateMachine>().ChangeState(Instantiate(enemy.chaseState));
                }
            }
        }
    }
}