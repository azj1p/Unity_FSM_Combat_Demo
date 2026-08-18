using UnityEngine;

[CreateAssetMenu(fileName = "EnemyVulnerableState", menuName = "FSM/Enemy/VulnerableState")]
public class EnemyVulnerableState : State, IDamageModifier
{
    [Header("Vulnerable Settings")]
    [Tooltip("破韧虚弱持续时间（秒）")]
    public float vulnerableDuration = 3.0f;
    [Tooltip("破韧状态下的受击伤害倍率")]
    public float damageMultiplier = 1.25f;

    private float timer;

    public override void OnEnter(StateMachine stateMachine)
    {
        timer = vulnerableDuration;
        var controller = stateMachine.GetComponent<EnemyController>();
        if (controller != null)
        {
            controller.isVulnerable = true;
            controller.SetVulnerableVisual(true); // 开启虚弱变色
        }

        var animator = stateMachine.GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("Vulnerable");
        }

        Debug.Log($"【FSM驱动】怪物进入破韧硬直状态，持续 {vulnerableDuration} 秒！");
    }

    public override void LogicUpdate(StateMachine stateMachine)
    {
        timer -= Time.deltaTime;
    }

    public override void TransitionChecks(StateMachine stateMachine)
    {
        if (timer <= 0f)
        {
            var controller = stateMachine.GetComponent<EnemyController>();
            if (controller != null)
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
            controller.SetVulnerableVisual(false); // 恢复初始原色
        }
    }

    public float ModifyDamage(float baseDamage)
    {
        float modified = baseDamage * damageMultiplier;
        Debug.Log($"【IDamageModifier】触发破韧易伤修饰！基础: {baseDamage} -> 修饰后: {modified} ({damageMultiplier}x)");
        return modified;
    }
}