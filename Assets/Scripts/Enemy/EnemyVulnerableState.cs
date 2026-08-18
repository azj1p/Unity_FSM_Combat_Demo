using UnityEngine;

[CreateAssetMenu(fileName = "EnemyVulnerableState", menuName = "FSM/Enemy/VulnerableState")]
public class EnemyVulnerableState : State, IDamageModifier
{
    [Header("Vulnerable Settings")]
    [Tooltip("破韧虚弱持续时间（秒）")]
    public float vulnerableDuration = 3.0f;
    [Tooltip("破韧状态下的受击伤害倍率")]
    public float damageMultiplier = 1.25f;

    public override void OnEnter(StateMachine stateMachine)
    {
        var controller = stateMachine.GetComponent<EnemyController>();
        if (controller != null)
        {
            controller.isVulnerable = true;
            controller.vulnerableTimer = vulnerableDuration; // 给当前怪物独立的计时器赋值
            controller.SetVulnerableVisual(true);
        }

        var animator = stateMachine.GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("Vulnerable");
        }

        Debug.Log($"【FSM驱动】怪物 [{stateMachine.name}] 进入破韧状态，独立计时 {vulnerableDuration} 秒！");
    }

    public override void LogicUpdate(StateMachine stateMachine)
    {
        var controller = stateMachine.GetComponent<EnemyController>();
        if (controller != null)
        {
            // 扣减当前怪物自己的计时器
            controller.vulnerableTimer -= Time.deltaTime;
        }
    }

    public override void TransitionChecks(StateMachine stateMachine)
    {
        var controller = stateMachine.GetComponent<EnemyController>();
        if (controller != null)
        {
            // 仅根据当前怪物自己的计时器判断是否恢复
            if (controller.vulnerableTimer <= 0f)
            {
                controller.ResetToughness();
                if (controller.chaseState != null)
                {
                    stateMachine.ChangeState(controller.chaseState);
                }
            }
        }
    }

    public override void OnExit(StateMachine stateMachine)
    {
        var controller = stateMachine.GetComponent<EnemyController>();
        if (controller != null)
        {
            controller.isVulnerable = false;
            controller.SetVulnerableVisual(false);
        }
    }

    public float ModifyDamage(float baseDamage)
    {
        return baseDamage * damageMultiplier;
    }
}