using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
partial struct HealthBarSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach ((
                     RefRW<LocalTransform> localTransform,
                     RefRW<Health> health,
                     RefRO<HealthBar> healthBar)
                 in SystemAPI.Query<
                     RefRW<LocalTransform>,
                     RefRW<Health>,
                     RefRO<HealthBar>>())
        {
            float healthNormalized = health.ValueRW.healthAmount / (float)health.ValueRW.healthAmountMax;
            Debug.Log("Health:" + healthNormalized);

            RefRW<PostTransformMatrix> barVisualPostTransformMatrix =
                SystemAPI.GetComponentRW<PostTransformMatrix>(healthBar.ValueRO.barVisualEntity);
            barVisualPostTransformMatrix.ValueRW.Value = float4x4.Scale(healthNormalized, 1, 1);
        }
    }
}