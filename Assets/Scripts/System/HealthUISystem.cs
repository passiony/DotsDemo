using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial struct HealthUISystem : ISystem
{
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
            
        }
    }
}