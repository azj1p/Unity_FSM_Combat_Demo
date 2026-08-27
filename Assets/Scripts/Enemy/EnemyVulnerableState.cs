using UnityEngine;

[CreateAssetMenu(fileName = "SO_EnemyVulnerable", menuName = "FSM/Enemy States/Vulnerable")]
public class EnemyVulnerableState : State<EnemyController>, IDamageModifier
{
    [Header("破韧配置")]
    [Tooltip("破韧虚弱状态持续时间（秒）")]
    [SerializeField] private float vulnerableDuration = 5.0f;
    [Tooltip("破韧期间受击增伤倍率（如 1.5 为 150% 伤害）")]
    [SerializeField] private float damageMultiplier = 1.5f;
    [Tooltip("复原时索敌警戒半径（米）")]
    [SerializeField] private float detectRadiusOnRecover = 12.0f;

    public override void OnEnter(EnemyController runner)
    {
        runner.vulnerableTimer = vulnerableDuration;
        runner.SetVulnerableVisual(true);

        if (runner.rb != null)
        {
            runner.rb.linearVelocity = Vector3.zero;
        }

        Debug.Log($"【FSM】怪物 [{runner.gameObject.name}] 进入破韧状态，独立计时 {vulnerableDuration} 秒！");
    }

    public override void LogicUpdate(EnemyController runner)
    {
        if (runner.isDead) return;

        runner.vulnerableTimer -= Time.deltaTime;
        if (runner.vulnerableTimer <= 0f)
        {
            // 1. 恢复视觉与韧性
            runner.SetVulnerableVisual(false);
            runner.ResetToughness();

            // 2. 智能恢复状态（优先判定追击/巡逻）
            RecoverFromVulnerable(runner);
        }
    }

    private void RecoverFromVulnerable(EnemyController runner)
    {
        if (runner.stateMachine == null) return;

        // 如果玩家存在且在警戒范围内，立即切入追击状态
        if (runner.playerTransform != null && runner.chaseState != null)
        {
            float dist = Vector3.Distance(runner.transform.position, runner.playerTransform.position);
            if (dist <= detectRadiusOnRecover)
            {
                runner.stateMachine.ChangeState(runner.chaseState);
                return;
            }
        }

        // 玩家不在身边时，优先恢复巡逻，巡逻为空再切待机
        if (runner.patrolState != null)
        {
            runner.stateMachine.ChangeState(runner.patrolState);
        }
        else if (runner.idleState != null)
        {
            runner.stateMachine.ChangeState(runner.idleState);
        }
    }

    public float ModifyDamage(float damage)
    {
        return damage * damageMultiplier;
    }
}