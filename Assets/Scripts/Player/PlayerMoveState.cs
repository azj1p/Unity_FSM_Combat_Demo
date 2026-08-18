using UnityEngine;

[CreateAssetMenu(fileName = "PlayerMoveState", menuName = "FSM/Player/MoveState")]
public class PlayerMoveState : State
{
    public override void LogicUpdate(StateMachine stateMachine)
    {
        var controller = stateMachine.GetComponent<PlayerController>();
        if (controller == null) return;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        if (h == 0 && v == 0)
        {
            if (Input.GetKey(KeyCode.W)) v += 1f;
            if (Input.GetKey(KeyCode.S)) v -= 1f;
            if (Input.GetKey(KeyCode.D)) h += 1f;
            if (Input.GetKey(KeyCode.A)) h -= 1f;
        }

        Vector3 inputDir = new Vector3(h, 0, v).normalized;

        if (inputDir.magnitude > 0.05f)
        {
            Vector3 moveDir = inputDir;
            if (Camera.main != null)
            {
                Vector3 camForward = Camera.main.transform.forward;
                Vector3 camRight = Camera.main.transform.right;
                camForward.y = 0f;
                camRight.y = 0f;
                camForward.Normalize();
                camRight.Normalize();
                moveDir = (camForward * inputDir.z + camRight * inputDir.x).normalized;
            }

            if (moveDir != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(moveDir);
                stateMachine.transform.rotation = Quaternion.Slerp(
                    stateMachine.transform.rotation,
                    targetRot,
                    15f * Time.deltaTime
                );
            }

            var rb = stateMachine.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 targetPos = rb.position + moveDir * (controller.moveSpeed * Time.deltaTime);
                rb.MovePosition(targetPos);
            }
            else
            {
                stateMachine.transform.position += moveDir * (controller.moveSpeed * Time.deltaTime);
            }
        }
    }

    public override void TransitionChecks(StateMachine stateMachine)
    {
        var controller = stateMachine.GetComponent<PlayerController>();
        if (controller == null) return;

        // 跳跃检测 (Space 键 + 地面检测)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var rb = stateMachine.GetComponent<Rigidbody>();
            if (rb != null && Physics.Raycast(stateMachine.transform.position, Vector3.down, 1.2f))
            {
                rb.AddForce(Vector3.up * controller.jumpForce, ForceMode.Impulse);
            }
        }

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        bool hasInput = (Mathf.Abs(h) > 0.01f || Mathf.Abs(v) > 0.01f) ||
                        Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) ||
                        Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D);

        if (!hasInput && controller.idleState != null)
        {
            stateMachine.ChangeState(controller.idleState);
            return;
        }

        if (Input.GetKeyDown(KeyCode.J) || Input.GetMouseButtonDown(0))
        {
            if (controller.attackState != null)
            {
                stateMachine.ChangeState(controller.attackState);
            }
        }
    }
}