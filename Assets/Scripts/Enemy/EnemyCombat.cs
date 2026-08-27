using UnityEngine;

/// <summary>
/// 负责战斗结算、共鸣增伤倍率与索敌目标管理
/// </summary>
public class EnemyCombat : MonoBehaviour
{
    [Header("Target (优先拖拽，留空则自动查找)")]
    public Transform playerTransform;

    private void Start()
    {
        if (playerTransform == null)
        {
            var playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null) playerTransform = playerObj.transform;
        }
    }

    public float CalculateOutgoingDamage(float baseDamage)
    {
        if (EnvironmentalResonance.Instance != null)
        {
            return baseDamage * EnvironmentalResonance.Instance.GetDamageBonusMultiplier();
        }
        return baseDamage;
    }
}