using System;
using UnityEngine;

public class StateMachine : MonoBehaviour
{
    public State initialState;
    public State CurrentState { get; private set; }

    // 事件系统：解耦状态转换监听（Suggestion #2）
    public event Action<State, State> OnStateChanged; // (原状态, 新状态)

    private MonoBehaviour runner;

    private void Awake()
    {
        runner = GetComponent<EnemyController>() as MonoBehaviour
              ?? GetComponent<PlayerController>() as MonoBehaviour
              ?? this;
    }

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
            CurrentState.ExecuteLogicUpdate(runner);
            CurrentState.ExecuteTransitionChecks(runner);
        }
    }

    private void FixedUpdate()
    {
        if (CurrentState != null)
        {
            CurrentState.ExecutePhysicsUpdate(runner);
        }
    }

    public void ChangeState(State newState)
    {
        if (newState == null || newState == CurrentState) return;

        State previousState = CurrentState;

        if (CurrentState != null)
        {
            CurrentState.ExecuteExit(runner);
        }

        CurrentState = newState;
        CurrentState.ExecuteEnter(runner);

        // 广播状态转换事件
        OnStateChanged?.Invoke(previousState, newState);
    }
}