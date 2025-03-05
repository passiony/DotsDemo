using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public enum Faction
{
    Red,
    Blue,
}

/// <summary>
/// 单位Baker
/// </summary>
class UnitAuthoring : MonoBehaviour
{
    public Faction faction;

    class Baker : Baker<UnitAuthoring>
    {
        public override void Bake(UnitAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Unit
            {
                Name=authoring.name,
                faction = authoring.faction,
            });
        }
    }
}

public struct Unit : IComponentData
{
    public FixedString32Bytes Name;
    public Faction faction;
}