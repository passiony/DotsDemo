using Unity.Entities;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 射击远攻
/// </summary>
public class MiningAuthoring : MonoBehaviour
{
    public float timerMax = 2;
    public int momeySpeed = 5;
    public float colliderSize = 2;

    private class Baker : Baker<MiningAuthoring>
    {
        public override void Bake(MiningAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Mining()
            {
                timerMax = authoring.timerMax,
                momeySpeed = authoring.momeySpeed,
                colliderSize = authoring.colliderSize,
            });
        }
    }
}

public struct Mining : IComponentData
{
    public float timer;
    public float timerMax;
    public int momeySpeed;
    public float colliderSize;
}