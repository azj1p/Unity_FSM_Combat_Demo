using UnityEngine;

[CreateAssetMenu(fileName = "EnemyDeadState", menuName = "FSM/Enemy/DeadState")]
public class EnemyDeadState : State<EnemyController>
{
    [Header("Death Settings")]
    public float destroyDelay = 1.5f;

    public override void OnEnter(EnemyController enemy)
    {
        if (enemy == null) return;
        enemy.isDead = true;
        enemy.HideUI();

        if (enemy.rb != null)
        {
            enemy.rb.linearVelocity = Vector3.zero;
            enemy.rb.isKinematic = true;
        }

        var col = enemy.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }

        var animator = enemy.GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        Debug.Log("【FSM驱动】敌人死亡，隐藏UI并等待销毁！");
        Destroy(enemy.gameObject, destroyDelay);
    }
}