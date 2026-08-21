using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "PlayerIdleState", menuName = "FSM/Player/IdleState")]
public class PlayerIdleState : State<PlayerController>
{
    public override void LogicUpdate(PlayerController player)
    {
        if (player == null) return;

        // 跳跃检测（新输入系统：Space 键 + 地面判定）
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (player.rb != null && Mathf.Abs(player.rb.linearVelocity.y) < 0.1f)
            {
                player.rb.AddForce(Vector3.up * player.jumpForce, ForceMode.Impulse);
            }
        }
    }

    public override void TransitionChecks(PlayerController player)
    {
        if (player == null) return;

        // 攻击触发检测（J 键或鼠标左键）
        bool attackPressed = (Keyboard.current != null && Keyboard.current.jKey.wasPressedThisFrame)
                          || (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame);

        if (attackPressed && player.attackState != null)
        {
            player.stateMachine.ChangeState(player.attackState);
            return;
        }

        // 移动触发检测（WASD / 方向键）
        bool isMoving = false;
        if (Keyboard.current != null)
        {
            isMoving = Keyboard.current.wKey.isPressed || Keyboard.current.sKey.isPressed ||
                       Keyboard.current.aKey.isPressed || Keyboard.current.dKey.isPressed;
        }

        if (isMoving && player.moveState != null)
        {
            player.stateMachine.ChangeState(player.moveState);
        }
    }
}