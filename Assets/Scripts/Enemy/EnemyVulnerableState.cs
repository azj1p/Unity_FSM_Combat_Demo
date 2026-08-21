using UnityEngine;

[CreateAssetMenu(fileName = "EnemyVulnerableState", menuName = "FSM/Enemy/VulnerableState")]
public class EnemyVulnerableState : State<EnemyController>, IDamageModifier
{
    [Header("Vulnerable Settings")]
    public float vulnerableDuration = 3.0f;
    public float damageMultiplier = 1.25f;

    public override void OnEnter(EnemyController enemy)
    {
        if (enemy == null) return;
        enemy.isVulnerable = true;
        enemy.vulnerableTimer = vulnerableDuration;
        enemy.SetVulnerableVisual(true);

        var animator = enemy.GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("Vulnerable");
        }

        Debug.Log($"【FSM驱动】怪物 [{enemy.name}] 进入破韧状态，独立计时 {vulnerableDuration} 秒！");
    }

    public override void LogicUpdate(EnemyController enemy)
    {
        if (enemy != null)
        {
            enemy.vulnerableTimer -= Time.deltaTime;
        }
    }

    public override void TransitionChecks(EnemyController enemy)
    {
        if (enemy == null) return;
        if (enemy.vulnerableTimer <= 0f)
        {
            enemy.ResetToughness();
            if (enemy.chaseState != null)
            {
                enemy.stateMachine.ChangeState(enemy.chaseState);
            }
        }
    }

    public override void OnExit(EnemyController enemy)
    {
        if (enemy == null) return;
        enemy.isVulnerable = false;
        enemy.SetVulnerableVisual(false);
    }

    public float ModifyDamage(float baseDamage)
    {
        return baseDamage * damageMultiplier;
    }
}