using UnityEngine;

// 【敌人状态】待机状态，原地等待一定时间或检测玩家靠近
[CreateAssetMenu(fileName = "EnemyIdleState", menuName = "FSM/States/EnemyIdleState")]
public class EnemyIdleState : State
{
    public override void OnEnter(StateMachine stateMachine)
    {
        Debug.Log("【FSM驱动】怪物进入待机状态 (Idle)");
    }
}