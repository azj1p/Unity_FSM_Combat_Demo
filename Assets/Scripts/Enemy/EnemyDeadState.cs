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
        }

        // 1. 冻结刚体物理，防止关闭碰撞体后穿透地面下落
        var rb = stateMachine.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero; // 清空残余速度
            rb.isKinematic = true;            // 设为运动学，不受重力影响
        }

        // 2. 禁用碰撞体，防止死后继续阻挡玩家或受击
        var col = stateMachine.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }

        // 3. 触发死亡动画
        var animator = stateMachine.GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        Debug.Log("【FSM驱动】敌人进入死亡状态 (DeadState)，停留在原地准备销毁！");

        // 4. 延迟销毁游戏对象
        Destroy(stateMachine.gameObject, destroyDelay);
    }
}