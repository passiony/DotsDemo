using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class ShootVictimAuthoring : MonoBehaviour
{

    public class Baker : Baker<ShootVictimAuthoring>
    {
        public override void Bake(ShootVictimAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new ShootVictim
            {
                hitLocalPosition = new Vector3(0, 0.75f, 0)
            });
        }
    }
}


public struct ShootVictim : IComponentData
{
    public float3 hitLocalPosition;
}