using Unity.Entities;
using UnityEngine;

public class HealthAuthoring : MonoBehaviour
{
    public Faction faction;
    public bool IsCity;
    public int healthAmount = 100;
    public int healthAmountMax = 100;

    private class HealthAuthoringBaker : Baker<HealthAuthoring>
    {
        public override void Bake(HealthAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Health
            {
                faction = authoring.faction,
                IsCity = authoring.IsCity,
                healthAmount = authoring.healthAmount,
                healthAmountMax = authoring.healthAmountMax,
                onHealthChanged = false,
            });
        }
    }
}

public struct Health : IComponentData
{
    public Faction faction;
    public bool IsCity;
    public int healthAmount;
    public int healthAmountMax;
    public bool onHealthChanged;
}