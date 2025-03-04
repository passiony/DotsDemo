using Unity.Entities;
using UnityEngine;

public class GoldAuthoring : MonoBehaviour
{
    public Faction faction;
    public int goldAmount = 100;
    
    private class Baker : Baker<GoldAuthoring>
    {
        public override void Bake(GoldAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Gold
            {
                faction = authoring.faction,
                goldAmount = authoring.goldAmount,
                onGoldChanged = true,
            });
        }
    }
}

public struct Gold : IComponentData
{
    public Faction faction;
    public int goldAmount;
    public bool onGoldChanged;
}