using UnityEngine;

[CreateAssetMenu(fileName = "EnemyDeadState", menuName = "FSM/Enemy/DeadState")]
public class EnemyDeadState : State
{
    [Header("Death Settings")]
    [Tooltip("死亡后销毁物体的延迟时间（秒）")]
    public float destroyDelay = 1.5f;

    public override void OnEnter(StateMachine stateMachine)
    {
        var controller = stateMachine.GetComponent<EnemyController>();
        if (controller != null)
        {
            controller.isDead = true;
            controller.HideUI(); // 死亡瞬间立即隐藏头顶血条和韧性条
        }

        // 冻结刚体物理
        var rb = stateMachine.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        // 禁用碰撞体
        var col = stateMachine.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }

        // 触发死亡动画
        var animator = stateMachine.GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        Debug.Log("【FSM驱动】敌人死亡，隐藏UI并等待销毁！");
        Destroy(stateMachine.gameObject, destroyDelay);
    }


}