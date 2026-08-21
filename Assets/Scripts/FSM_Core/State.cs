using UnityEngine;

// 非泛型基类：保证 Unity Inspector 可以正常拖拽与多态持有
public abstract class State : ScriptableObject
{
    public abstract void ExecuteEnter(MonoBehaviour runner);
    public abstract void ExecuteLogicUpdate(MonoBehaviour runner);
    public abstract void ExecutePhysicsUpdate(MonoBehaviour runner);
    public abstract void ExecuteTransitionChecks(MonoBehaviour runner);
    public abstract void ExecuteExit(MonoBehaviour runner);
}

// 泛型抽象基类：直接向子类传递强类型 runner，彻底消除 GetComponent
public abstract class State<T> : State where T : MonoBehaviour
{
    public override void ExecuteEnter(MonoBehaviour runner) { if (runner is T target) OnEnter(target); }
    public override void ExecuteLogicUpdate(MonoBehaviour runner) { if (runner is T target) LogicUpdate(target); }
    public override void ExecutePhysicsUpdate(MonoBehaviour runner) { if (runner is T target) PhysicsUpdate(target); }
    public override void ExecuteTransitionChecks(MonoBehaviour runner) { if (runner is T target) TransitionChecks(target); }
    public override void ExecuteExit(MonoBehaviour runner) { if (runner is T target) OnExit(target); }

    public virtual void OnEnter(T runner) { }
    public virtual void LogicUpdate(T runner) { }
    public virtual void PhysicsUpdate(T runner) { }
    public virtual void TransitionChecks(T runner) { }
    public virtual void OnExit(T runner) { }
}