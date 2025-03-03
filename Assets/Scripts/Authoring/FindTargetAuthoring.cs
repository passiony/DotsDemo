using Unity.Entities;
using UnityEngine;


public class FindTargetAuthoring : MonoBehaviour
{
    public Faction targetFaction;

    private class FindTargetAuthoringBaker : Baker<FindTargetAuthoring>
    {
        public override void Bake(FindTargetAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new FindTarget {
                targetFaction = authoring.targetFaction,
            });
        }
    }
}

public struct FindTarget : IComponentData
{
    public Faction targetFaction;
}