using UnityEngine;

[CreateAssetMenu(fileName = "PlayerIdleState", menuName = "FSM/Player/IdleState")]
public class PlayerIdleState : State
{
    public override void OnEnter(StateMachine stateMachine)
    {
        var rb = stateMachine.GetComponent<Rigidbody>();
        if (rb != null && !rb.isKinematic)
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }
    }

    public override void TransitionChecks(StateMachine stateMachine)
    {
        var controller = stateMachine.GetComponent<PlayerController>();
        if (controller == null) return;

        // 空格跳跃
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
        bool isMoving = (Mathf.Abs(h) > 0.01f || Mathf.Abs(v) > 0.01f) ||
                        Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) ||
                        Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D);

        if (isMoving && controller.moveState != null)
        {
            stateMachine.ChangeState(controller.moveState);
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