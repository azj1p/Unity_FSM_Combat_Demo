using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "PlayerMoveState", menuName = "FSM/Player/MoveState")]
public class PlayerMoveState : State<PlayerController>
{
    public override void LogicUpdate(PlayerController player)
    {
        if (player == null) return;

        // 1. 读取输入
        float h = 0f;
        float v = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) h -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) h += 1f;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) v += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) v -= 1f;
        }

        // 2. 基于摄像机朝向投影计算移动方向（工业级第三人称视口对齐）
        Transform cam = Camera.main != null ? Camera.main.transform : null;
        Vector3 moveDir = Vector3.zero;

        if (cam != null)
        {
            Vector3 camForward = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
            Vector3 camRight = Vector3.ProjectOnPlane(cam.right, Vector3.up).normalized;
            moveDir = (camForward * v + camRight * h).normalized;
        }
        else
        {
            moveDir = new Vector3(h, 0, v).normalized;
        }

        if (moveDir.magnitude > 0.1f)
        {
            player.transform.position += moveDir * player.moveSpeed * Time.deltaTime;
            player.transform.forward = moveDir;
        }

        // 3. 移动中跳跃检测
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

        bool attackPressed = (Keyboard.current != null && Keyboard.current.jKey.wasPressedThisFrame)
                          || (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame);

        if (attackPressed && player.attackState != null)
        {
            player.stateMachine.ChangeState(player.attackState);
            return;
        }

        bool isMoving = false;
        if (Keyboard.current != null)
        {
            isMoving = Keyboard.current.wKey.isPressed || Keyboard.current.sKey.isPressed ||
                       Keyboard.current.aKey.isPressed || Keyboard.current.dKey.isPressed;
        }

        if (!isMoving && player.idleState != null)
        {
            player.stateMachine.ChangeState(player.idleState);
        }
    }
}