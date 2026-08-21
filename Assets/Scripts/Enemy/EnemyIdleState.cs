using UnityEngine;

[CreateAssetMenu(fileName = "EnemyIdleState", menuName = "FSM/States/EnemyIdleState")]
public class EnemyIdleState : State<EnemyController>
{
    public override void OnEnter(EnemyController enemy)
    {
        Debug.Log("【FSM驱动】怪物进入待机状态 (Idle)");
    }
}