using UnityEngine;

[CreateAssetMenu(fileName = "EnemyChaseState", menuName = "FSM/Enemy/ChaseState")]
public class EnemyChaseState : State
{
    public float chaseSpeed = 3.5f;
    public float attackRange = 1.5f;

    public override void LogicUpdate(StateMachine stateMachine)
    {
        var controller = stateMachine.GetComponent<EnemyController>();
        if (controller == null || controller.playerTransform == null) return;

        Vector3 targetPos = controller.playerTransform.position;
        targetPos.y = stateMachine.transform.position.y;

        // 转向目标
        stateMachine.transform.LookAt(targetPos);

        // 使用物理刚体位移防止穿模重叠
        var rb = stateMachine.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 nextPos = Vector3.MoveTowards(
                rb.position,
                targetPos,
                chaseSpeed * Time.deltaTime
            );
            rb.MovePosition(nextPos);
        }
        else
        {
            stateMachine.transform.position = Vector3.MoveTowards(
                stateMachine.transform.position,
                targetPos,
                chaseSpeed * Time.deltaTime
            );
        }
    }

    public override void TransitionChecks(StateMachine stateMachine)
    {
        var controller = stateMachine.GetComponent<EnemyController>();
        if (controller == null || controller.playerTransform == null) return;

        float distance = Vector3.Distance(stateMachine.transform.position, controller.playerTransform.position);
        if (distance <= attackRange && controller.attackState != null)
        {
            stateMachine.ChangeState(controller.attackState);
        }
    }
}