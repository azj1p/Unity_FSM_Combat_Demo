/// <summary>
/// P3-2 行动值系统接入实体接口
/// 统一约束玩家、敌人或特殊战斗单位的行动速率与就绪回调
/// </summary>
public interface IActionValueEntity
{
    /// <summary>
    /// 行动速度（决定行动槽累加速度，基准为 100/周期）
    /// </summary>
    float ActionSpeed { get; }

    /// <summary>
    /// 行动值蓄满（达到 100）就绪时的回调逻辑
    /// </summary>
    void ExecuteAction();
}