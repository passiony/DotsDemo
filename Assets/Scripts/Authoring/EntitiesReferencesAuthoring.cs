using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class EntitiesReferencesAuthoring : MonoBehaviour
{
    public GameObject redSoldierPrefab;
    public GameObject blueSoldierPrefab;
    public GameObject redShootPrefab;
    public GameObject blueShootPrefab;
    public GameObject redMiningPrefab;
    public GameObject blueMiningPrefab;
    
    public Transform redStartPoint;
    public Transform blueStartPoint;
    public Transform redMinePoint;
    public Transform blueMinePoint;
    
    private class Baker : Baker<EntitiesReferencesAuthoring>
    {
        public override void Bake(EntitiesReferencesAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new EntitiesReferences
            {
                redMeleePrefabEntity = GetEntity(authoring.redSoldierPrefab, TransformUsageFlags.Dynamic),
                blueMeleePrefabEntity = GetEntity(authoring.blueSoldierPrefab, TransformUsageFlags.Dynamic),
                redShootPrefabEntity = GetEntity(authoring.redShootPrefab, TransformUsageFlags.Dynamic),
                blueShootPrefabEntity = GetEntity(authoring.blueShootPrefab, TransformUsageFlags.Dynamic),
                redMiningPrefabEntity = GetEntity(authoring.redMiningPrefab, TransformUsageFlags.Dynamic),
                blueMiningPrefabEntity = GetEntity(authoring.blueMiningPrefab, TransformUsageFlags.Dynamic),
                redStartPosition = authoring.redStartPoint.position,
                blueStartPosition = authoring.blueStartPoint.position,
                redMinePosition = authoring.redMinePoint.position,
                blueMinePosition = authoring.blueMinePoint.position
            });
        }
    }
}

public struct EntitiesReferences : IComponentData
{
    public Entity redMeleePrefabEntity;
    public Entity blueMeleePrefabEntity;
    public Entity redShootPrefabEntity;
    public Entity blueShootPrefabEntity;
    public Entity redMiningPrefabEntity;
    public Entity blueMiningPrefabEntity;
    public float3 redStartPosition;
    public float3 blueStartPosition;
    public float3 redMinePosition;
    public float3 blueMinePosition;
}