using UnityEngine;
// 【FSM核心】所有状态的抽象基类，定义 OnEnter、LogicUpdate、TransitionChecks、OnExit 生命周期回调
// 基于 ScriptableObject 的抽象状态基类
public abstract class State : ScriptableObject
{
    // 进入状态时执行一次
    public virtual void OnEnter(StateMachine stateMachine) { }

    // 退出状态时执行一次
    public virtual void OnExit(StateMachine stateMachine) { }

    // 每帧逻辑更新 (Update)
    public virtual void LogicUpdate(StateMachine stateMachine) { }

    // 物理帧更新 (FixedUpdate)
    public virtual void PhysicsUpdate(StateMachine stateMachine) { }

    // 状态切换条件检测
    public virtual void TransitionChecks(StateMachine stateMachine) { }
}