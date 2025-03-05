using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;


public class FindTargetAuthoring : MonoBehaviour
{
    public float range = 50;
    public Faction targetFaction;
    public float timerMax = 2;

    private class FindTargetAuthoringBaker : Baker<FindTargetAuthoring>
    {
        public override void Bake(FindTargetAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new FindTarget
            {
                range = authoring.range,
                targetFaction = authoring.targetFaction,
                timerMax = authoring.timerMax,
            });
        }
    }
}

public struct FindTarget : IComponentData
{
    public float range;
    public Faction targetFaction;
    public float timer;
    public float timerMax;
    public Entity targetEntity;
}