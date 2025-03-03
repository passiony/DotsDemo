using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial struct HealthUISystem : ISystem
{
    private NativeQueue<int>.ParallelWriter _writer;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        _writer = Facade.SharedQueue.AsParallelWriter();
    }
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach ((RefRW<PostTransformMatrix> localTransform,
                     RefRO<HealthUI> healthBar)
                 in SystemAPI.Query<
                     RefRW<PostTransformMatrix>,
                     RefRO<HealthUI>>())
        {
            Health health = SystemAPI.GetComponent<Health>(healthBar.ValueRO.healthEntity);
            if (!health.onHealthChanged)
            {
                continue;
            }

            health.onHealthChanged = false;
            float healthNormalized = health.healthAmount / (float)health.healthAmountMax;
#if UNITY_EDITOR
            Debug.Log(health.faction + ":" + health.healthAmount);
#endif
            localTransform.ValueRW.Value = float4x4.Scale(healthNormalized, 1, 1);
            _writer.Enqueue(health.healthAmount);
        }
    }
}