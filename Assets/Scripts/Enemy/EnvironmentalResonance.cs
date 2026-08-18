using UnityEngine;

public class EnvironmentalResonance : MonoBehaviour
{
    [Header("Resonance Settings")]
    public int resonanceStacks = 0;
    public int maxResonanceStacks = 3;
    public float resonanceDamageBonus = 0.1f;
    public float resonanceInterval = 6.0f;
    public float aoeRadius = 8.0f;

    private float resonanceTimer;
    private EnemyController enemyController;

    private void Awake()
    {
        enemyController = GetComponent<EnemyController>();
        resonanceTimer = resonanceInterval;
    }

    private void Update()
    {
        if (enemyController != null && (enemyController.isDead || enemyController.isVulnerable))
        {
            return;
        }

        resonanceTimer -= Time.deltaTime;
        if (resonanceTimer <= 0f)
        {
            resonanceTimer = resonanceInterval;
            if (resonanceStacks < maxResonanceStacks)
            {
                resonanceStacks++;
                Debug.Log($"【环境共鸣】层数累加: {resonanceStacks}/{maxResonanceStacks} (+{resonanceStacks * resonanceDamageBonus * 100}% 增伤)");

                if (resonanceStacks >= maxResonanceStacks)
                {
                    TriggerResonanceAOE();
                }
            }
        }
    }

    public void ResetResonance()
    {
        resonanceStacks = 0;
        resonanceTimer = resonanceInterval;
        Debug.Log("【环境共鸣】共鸣层数已重置！");
    }

    public float GetDamageBonusMultiplier()
    {
        return 1f + (resonanceStacks * resonanceDamageBonus);
    }

    public void TriggerResonanceAOE()
    {
        Debug.LogWarning("【共鸣爆发】释放 8m AOE 爆发伤害！");
        if (enemyController != null && enemyController.playerTransform != null)
        {
            float dist = Vector3.Distance(transform.position, enemyController.playerTransform.position);
            if (dist <= aoeRadius)
            {
                var damageable = enemyController.playerTransform.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(30f);
                }
            }
        }
        resonanceStacks = 0;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, aoeRadius);
    }
}