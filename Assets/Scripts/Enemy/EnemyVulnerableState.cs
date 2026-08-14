// 【敌人状态】破韧脆硬直状态，韧性归零后触发，固定持续一定时间后恢复韧性
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyVulnerableState", menuName = "FSM/Enemy/VulnerableState")]
public class EnemyVulnerableState : State
{
    [Header("破韧参数")]
    public float vulnerableDuration = 3.0f; // 固定破韧硬直持续时间 (3 秒)
    private float timer;

    public override void OnEnter(StateMachine stateMachine)
    {
        timer = vulnerableDuration;

        var enemy = stateMachine.GetComponent<EnemyController>();
        if (enemy != null)
        {
            enemy.isVulnerable = true; // 进入破韧标记
            Debug.Log($"【FSM驱动】怪物进入破韧硬直状态，固定持续 {vulnerableDuration} 秒！");
        }
    }

    public override void LogicUpdate(StateMachine stateMachine)
    {
        // 独立倒计时：无论期间被攻击多少次，该倒计时均不受影响
        timer -= Time.deltaTime;
    }

    public override void TransitionChecks(StateMachine stateMachine)
    {
        // 倒计时结束后自动恢复韧性并切回追击/攻击状态
        if (timer <= 0)
        {
            var enemy = stateMachine.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.ResetToughness();
            }
        }
    }

    public override void OnExit(StateMachine stateMachine)
    {
        var enemy = stateMachine.GetComponent<EnemyController>();
        if (enemy != null)
        {
            enemy.isVulnerable = false;
        }
    }
}