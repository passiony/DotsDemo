using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class EntitiesReferencesAuthoring : MonoBehaviour
{
    public GameObject redSoldierPrefab;
    public GameObject blueSoldierPrefab;

    public Transform redStartPoint;
    public Transform blueStartPoint;

    private class Baker : Baker<EntitiesReferencesAuthoring>
    {
        public override void Bake(EntitiesReferencesAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new EntitiesReferences
            {
                redSoldierPrefabEntity = GetEntity(authoring.redSoldierPrefab, TransformUsageFlags.Dynamic),
                blueSoldierPrefabEntity = GetEntity(authoring.blueSoldierPrefab, TransformUsageFlags.Dynamic),
                redStartPosition = authoring.redStartPoint.position,
                blueStartPosition = authoring.blueStartPoint.position
            });
        }
    }
}

public struct EntitiesReferences : IComponentData
{
    public Entity redSoldierPrefabEntity;
    public Entity blueSoldierPrefabEntity;
    public float3 redStartPosition;
    public float3 blueStartPosition;
}