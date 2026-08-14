using UnityEngine;

// 【玩家状态】移动与跳跃状态，处理基于相机的相对移动方向、角色平滑转向及跳跃判定
[CreateAssetMenu(fileName = "PlayerMoveState", menuName = "FSM/Player/MoveState")]
public class PlayerMoveState : State
{
    public float jumpForce = 5f; // 跳跃力度

    public override void LogicUpdate(StateMachine stateMachine)
    {
        var player = stateMachine.GetComponent<PlayerController>();
        if (player == null) return;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 inputDir = new Vector3(h, 0, v).normalized;

        if (inputDir.magnitude >= 0.1f)
        {
            // 获取镜头朝向，计算相对于镜头的移动方向
            Camera mainCam = Camera.main;
            Vector3 camForward = mainCam.transform.forward;
            Vector3 camRight = mainCam.transform.right;
            camForward.y = 0;
            camRight.y = 0;
            camForward.Normalize();
            camRight.Normalize();

            Vector3 moveDir = (camForward * inputDir.z + camRight * inputDir.x).normalized;

            // 移动玩家
            player.transform.Translate(moveDir * player.moveSpeed * Time.deltaTime, Space.World);

            // 角色自动平滑转向当前移动的方向
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            player.transform.rotation = Quaternion.Slerp(player.transform.rotation, targetRotation, 15f * Time.deltaTime);
        }
    }

    public override void TransitionChecks(StateMachine stateMachine)
    {
        var player = stateMachine.GetComponent<PlayerController>();
        if (player == null) return;

        // 1. 切回待机 (Idle)
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        if (h == 0 && v == 0)
        {
            player.ChangeState(player.idleState);
            return;
        }

        // 2. 切入攻击 (鼠标左键)
        if (Input.GetMouseButtonDown(0))
        {
            player.ChangeState(player.attackState);
            return;
        }

        // 3. 跳跃处理 (Space 键)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null && Mathf.Abs(rb.linearVelocity.y) < 0.05f)
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }
        }
    }
}