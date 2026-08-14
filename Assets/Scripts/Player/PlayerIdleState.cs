using UnityEngine;

// 【玩家状态】待机状态，检测 WASD 移动与鼠标左键攻击输入
[CreateAssetMenu(fileName = "PlayerIdleState", menuName = "FSM/Player/IdleState")]
public class PlayerIdleState : State
{
    public override void TransitionChecks(StateMachine stateMachine)
    {
        var player = stateMachine.GetComponent<PlayerController>();
        if (player == null) return;

        // 检测 WASD 移动
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        if (h != 0 || v != 0)
        {
            player.ChangeState(player.moveState);
            return;
        }

        // 改为检测鼠标左键攻击 (GetMouseButtonDown(0))
        if (Input.GetMouseButtonDown(0))
        {
            player.ChangeState(player.attackState);
        }
    }
}