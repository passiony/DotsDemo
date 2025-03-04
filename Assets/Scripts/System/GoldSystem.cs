using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial struct GoldSystem : ISystem
{
    private NativeQueue<RcvData>.ParallelWriter _writer;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        _writer = Facade.SharedQueue.AsParallelWriter();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // foreach ((RefRW<PostTransformMatrix> localTransform,
        //              RefRW<Gold> health)
        //          in SystemAPI.Query<
        //              RefRW<PostTransformMatrix>,
        //              RefRW<Gold>>())
        foreach (RefRW<Gold> health in SystemAPI.Query<RefRW<Gold>>())
        {
            if (!health.ValueRW.onGoldChanged)
            {
                continue;
            }

            health.ValueRW.onGoldChanged = false;
            float healthNormalized = health.ValueRW.goldAmount;
            //Debug.Log(health.faction + ":" + health.healthAmount);
            // localTransform.ValueRW.Value = float4x4.Scale(healthNormalized, 1, 1);
            
            //发送数据到UI层
            _writer.Enqueue(new RcvData(health.ValueRW.faction, 2, healthNormalized));
        }
    }
}