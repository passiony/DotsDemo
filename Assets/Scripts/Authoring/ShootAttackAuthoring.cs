using Unity.Entities;
using UnityEngine;

/// <summary>
/// 射击远攻
/// </summary>
public class ShootAttackAuthoring : MonoBehaviour
{
    public float timerMax;
    public int damageAmount;
    public float colliderSize;

    private class Baker : Baker<ShootAttackAuthoring>
    {
        public override void Bake(ShootAttackAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new MeleeAttack
            {
                timerMax = authoring.timerMax,
                damageAmount = authoring.damageAmount,
                colliderSize = authoring.colliderSize,
            });
        }
    }
}

public struct ShootAttack : IComponentData
{
    public float timer;
    public float timerMax;
    public int damageAmount;
    public float colliderSize;
}