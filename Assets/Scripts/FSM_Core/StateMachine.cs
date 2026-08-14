using UnityEngine;

// 【FSM核心】状态机组件，挂载在 GameObject 上，负责驱动当前 State 的生命周期与状态切换
public class StateMachine : MonoBehaviour
{
    [Header("初始状态")]
    public State initialState;

    // 当前运行的状态（实例化后的独立对象）
    public State currentState { get; private set; }

    private void Start()
    {
        if (initialState != null)
        {
            // 实例化 SO 资产，保证每个怪物的状态运行时互不干扰
            ChangeState(Instantiate(initialState));
        }
    }

    private void Update()
    {
        if (currentState != null)
        {
            currentState.LogicUpdate(this);
            currentState.TransitionChecks(this);
        }
    }

    private void FixedUpdate()
    {
        if (currentState != null)
        {
            currentState.PhysicsUpdate(this);
        }
    }

    // 切换状态的核心函数
    public void ChangeState(State newState)
    {
        if (currentState != null)
        {
            currentState.OnExit(this);
        }

        currentState = newState;

        if (currentState != null)
        {
            currentState.OnEnter(this);
        }
    }
}