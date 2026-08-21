using UnityEngine;

[CreateAssetMenu(fileName = "EnemyAttackState", menuName = "FSM/Enemy/AttackState")]
public class EnemyAttackState : State<EnemyController>
{
    [Header("Attack Settings")]
    public float attackDuration = 1.0f;
    public float baseAttackDamage = 15.0f;
    public float attackRange = 2.0f;

    public override void OnEnter(EnemyController enemy)
    {
        if (enemy == null) return;
        enemy.attackTimer = attackDuration;

        if (enemy.rb != null && !enemy.rb.isKinematic)
        {
            enemy.rb.linearVelocity = new Vector3(0, enemy.rb.linearVelocity.y, 0);
        }

        if (enemy.playerTransform != null)
        {
            Vector3 lookTarget = enemy.playerTransform.position;
            lookTarget.y = enemy.transform.position.y;
            enemy.transform.LookAt(lookTarget);
        }

        var animator = enemy.GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        float finalDamage = enemy.GetCalculatedAttackDamage(baseAttackDamage);
        if (enemy.playerTransform != null)
        {
            float distance = Vector3.Distance(enemy.transform.position, enemy.playerTransform.position);
            if (distance <= attackRange)
            {
                var damageable = enemy.playerTransform.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(finalDamage);
                    Debug.Log($"【敌人攻击】命中玩家！造成 {finalDamage} 点伤害 (共鸣加成: {enemy.resonanceStacks} 层)");
                }
            }
        }
    }

    public override void LogicUpdate(EnemyController enemy)
    {
        if (enemy != null)
        {
            enemy.attackTimer -= Time.deltaTime;
        }
    }

    public override void TransitionChecks(EnemyController enemy)
    {
        if (enemy == null || enemy.playerTransform == null) return;

        // 攻击后摇结束
        if (enemy.attackTimer <= 0f)
        {
            float distance = Vector3.Distance(enemy.transform.position, enemy.playerTransform.position);

            // 玩家仍在攻击范围内：重置计时并执行下一次攻击（解决同状态无法切换的问题）
            if (distance <= attackRange)
            {
                OnEnter(enemy);
            }
            else if (enemy.chaseState != null)
            {
                enemy.stateMachine.ChangeState(enemy.chaseState);
            }
            else if (enemy.idleState != null)
            {
                enemy.stateMachine.ChangeState(enemy.idleState);
            }
        }
    }
}