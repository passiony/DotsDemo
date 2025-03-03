using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class UnitSpawnerAuthoring : MonoBehaviour
{
    private class Baker : Baker<UnitSpawnerAuthoring>
    {
        public override void Bake(UnitSpawnerAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new UnitSpawner
            {
                
            });
        }
    }
}

public struct UnitSpawner : IComponentData
{

}