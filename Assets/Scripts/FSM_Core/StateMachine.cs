using UnityEngine;

public class StateMachine : MonoBehaviour
{
    [Header("State Machine")]
    [SerializeField] private State initialState;

    // 公开当前状态属性，供外部（如 Controller、接口修饰器）安全读取
    public State CurrentState { get; private set; }

    private void Start()
    {
        if (initialState != null)
        {
            ChangeState(initialState);
        }
    }

    private void Update()
    {
        if (CurrentState != null)
        {
            CurrentState.LogicUpdate(this);
            CurrentState.TransitionChecks(this);
        }
    }

    private void FixedUpdate()
    {
        if (CurrentState != null)
        {
            CurrentState.PhysicsUpdate(this);
        }
    }

    public void ChangeState(State newState)
    {
        if (CurrentState != null)
        {
            CurrentState.OnExit(this);
        }

        CurrentState = newState;

        if (CurrentState != null)
        {
            CurrentState.OnEnter(this);
        }
    }
}