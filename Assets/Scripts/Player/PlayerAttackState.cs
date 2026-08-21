using UnityEngine;

[CreateAssetMenu(fileName = "PlayerAttackState", menuName = "FSM/Player/AttackState")]
public class PlayerAttackState : State<PlayerController>
{
    public float attackDuration = 0.5f;
    public float attackRadius = 2.0f;
    private float timer;

    public override void OnEnter(PlayerController player)
    {
        timer = attackDuration;

        var anim = player.GetComponent<Animator>();
        if (anim != null) anim.SetTrigger("Attack");

        Collider[] hits = Physics.OverlapSphere(player.transform.position + player.transform.forward, attackRadius);
        foreach (var hit in hits)
        {
            if (hit.gameObject != player.gameObject)
            {
                var damageable = hit.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(player.attackDamage, player.toughnessDamage);
                }
            }
        }
    }

    public override void LogicUpdate(PlayerController player)
    {
        timer -= Time.deltaTime;
    }

    public override void TransitionChecks(PlayerController player)
    {
        if (timer <= 0f && player.idleState != null)
        {
            player.stateMachine.ChangeState(player.idleState);
        }
    }
}