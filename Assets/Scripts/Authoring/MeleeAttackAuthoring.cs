using Unity.Entities;
using UnityEngine;

/// <summary>
/// 近战攻击
/// </summary>
public class MeleeAttackAuthoring : MonoBehaviour
{
    public float timerMax;
    public int damageAmount;
    public float colliderSize;

    private class Baker : Baker<MeleeAttackAuthoring>
    {
        public override void Bake(MeleeAttackAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new MeleeAttack
            {
                timer = authoring.timerMax,
                timerMax = authoring.timerMax,
                damageAmount = authoring.damageAmount,
                colliderSize = authoring.colliderSize,
            });
        }
    }
}

public struct MeleeAttack : IComponentData
{
    public float timer;
    public float timerMax;
    public int damageAmount;
    public float colliderSize;
}