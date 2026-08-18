public interface IDamageModifier
{
    // 由具体状态实现伤害修饰（如易伤倍率、减伤等）
    float ModifyDamage(float baseDamage);
}