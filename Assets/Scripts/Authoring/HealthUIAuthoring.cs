using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class HealthUIAuthoring : MonoBehaviour
{
    public GameObject healthEntity;

    private class Baker : Baker<HealthUIAuthoring>
    {
        public override void Bake(HealthUIAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.NonUniformScale);
            AddComponent(entity, new HealthUI
            {
                healthEntity = GetEntity(authoring.healthEntity, TransformUsageFlags.Dynamic),
            });
        }
    }
}

public struct HealthUI : IComponentData
{
    public Entity healthEntity;
}