using UnityEngine;

[CreateAssetMenu(fileName = "SO_EnemyIdle", menuName = "FSM/Enemy States/Idle")]
public class EnemyIdleState : State<EnemyController>
{
    [Header("待机配置")]
    [SerializeField] private float idleDuration = 2.0f;
    [SerializeField] private float chaseRange = 8.0f;

    private float timer;

    public override void OnEnter(EnemyController runner)
    {
        timer = idleDuration;
        if (runner.rb != null)
        {
            runner.rb.linearVelocity = Vector3.zero;
        }
    }

    public override void LogicUpdate(EnemyController runner)
    {
        if (runner.isDead) return;

        // 1. 优先检测玩家是否进入追击范围
        if (runner.playerTransform != null && runner.chaseState != null)
        {
            float dist = Vector3.Distance(runner.transform.position, runner.playerTransform.position);
            if (dist <= chaseRange)
            {
                runner.stateMachine.ChangeState(runner.chaseState);
                return;
            }
        }

        // 2. 待机倒计时结束，切回巡逻
        timer -= Time.deltaTime;
        if (timer <= 0f && runner.patrolState != null)
        {
            runner.stateMachine.ChangeState(runner.patrolState);
        }
    }
}