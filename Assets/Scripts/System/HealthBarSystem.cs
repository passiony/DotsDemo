using Unity.Burst;
using Unity.Collections;
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
        var postTransformLookup = SystemAPI.GetComponentLookup<PostTransformMatrix>();

        new HealthBarJob
        {
            PostTransformLookup = postTransformLookup
        }.ScheduleParallel();
    }
}

[BurstCompile]
public partial struct HealthBarJob : IJobEntity
{
    [NativeDisableParallelForRestriction] public ComponentLookup<PostTransformMatrix> PostTransformLookup;

    void Execute(ref LocalTransform localTransform,
        in Health health,
        in HealthBar healthBar)
    {
        if (!PostTransformLookup.HasComponent(healthBar.barVisualEntity))
            return;

        var matrix = PostTransformLookup.GetRefRW(healthBar.barVisualEntity);
        float healthNormalized = math.clamp(health.healthAmount / (float)health.healthAmountMax, 0, 1);
        matrix.ValueRW.Value = float4x4.Scale(healthNormalized, 1, 1);
    }
}