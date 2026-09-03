using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// P3-2 核心行动值系统：负责时间轴驱动、多实体行动累加与推条联动
/// </summary>
public class ActionValueSystem : MonoBehaviour
{
    public static ActionValueSystem Instance { get; private set; }

    [Header("核心行动值配置")]
    [SerializeField] private float baseActionValue = 100f; // 行动槽基数（基准 100）
    [SerializeField] private float intervalSeconds = 5f;   // 与共鸣周期保持一致 (5.0s)

    // 公开属性供外部访问与对齐周期（同时消除 CS0414 警告）
    public float BaseActionValue => baseActionValue;
    public float IntervalSeconds => intervalSeconds;

    // 实体进度字典：Entity -> 当前行动值 (0 ~ 100)
    private readonly Dictionary<IActionValueEntity, float> avMap = new();

    // 事件广播
    public event Action<IActionValueEntity, float> OnActionValueUpdated; // 实体, 进度比例 (0~1)
    public event Action<IActionValueEntity> OnActionReady;               // 实体行动就绪

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        if (avMap.Count == 0) return;

        // 获取环境共鸣的全局速度倍率联动
        float speedMultiplier = 1.0f;
        if (EnvironmentalResonance.Instance != null)
        {
            speedMultiplier = EnvironmentalResonance.Instance.GlobalSpeedMultiplier;
        }

        // 遍历所有实体并根据行动速度累加进度
        foreach (var entity in avMap.Keys.ToList())
        {
            if (entity == null)
            {
                avMap.Remove(entity);
                continue;
            }

            // 依据 ActionSpeed、DeltaTime 和全局加速倍率递增行动值
            avMap[entity] += entity.ActionSpeed * Time.deltaTime * speedMultiplier;

            // 行动值蓄满判定
            if (avMap[entity] >= baseActionValue)
            {
                avMap[entity] = baseActionValue;
                OnActionReady?.Invoke(entity);
                entity.ExecuteAction();

                // 行动执行后重置当前行动槽，开启新一轮累加
                avMap[entity] = 0f;
            }

            // 广播归一化进度 (0.0f ~ 1.0f) 供 UI 刷新
            float progress = Mathf.Clamp01(avMap[entity] / baseActionValue);
            OnActionValueUpdated?.Invoke(entity, progress);
        }
    }

    /// <summary>
    /// 注册战斗实体进入行动序列
    /// </summary>
    public void RegisterEntity(IActionValueEntity entity)
    {
        if (entity != null && !avMap.ContainsKey(entity))
        {
            avMap.Add(entity, 0f);
        }
    }

    /// <summary>
    /// 注销战斗实体
    /// </summary>
    public void UnregisterEntity(IActionValueEntity entity)
    {
        if (entity != null && avMap.ContainsKey(entity))
        {
            avMap.Remove(entity);
        }
    }

    /// <summary>
    /// 核心推条/延后接口：使实体的行动值延后指定百分比（等价映射破韧/硬直惩罚）
    /// </summary>
    public void DelayAction(IActionValueEntity entity, float delayPercent)
    {
        if (entity != null && avMap.ContainsKey(entity))
        {
            float penalty = baseActionValue * Mathf.Clamp01(delayPercent);
            avMap[entity] = Mathf.Max(0f, avMap[entity] - penalty);

            float progress = Mathf.Clamp01(avMap[entity] / baseActionValue);
            OnActionValueUpdated?.Invoke(entity, progress);
            Debug.Log($"【行动值延后】[{entity.GetType().Name}] 行动值被击退 {delayPercent * 100:F0}%，当前进度: {progress * 100:F1}%");
        }
    }

    /// <summary>
    /// 获取实体当前行动进度 (0.0f ~ 1.0f)
    /// </summary>
    public float GetActionProgress(IActionValueEntity entity)
    {
        if (entity != null && avMap.TryGetValue(entity, out float val))
        {
            return Mathf.Clamp01(val / baseActionValue);
        }
        return 0f;
    }
}