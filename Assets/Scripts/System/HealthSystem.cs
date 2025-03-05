using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial struct HealthSystem : ISystem
{
    private NativeQueue<RcvData>.ParallelWriter _writer;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        _writer = Facade.SharedQueue.AsParallelWriter();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // foreach ((RefRW<PostTransformMatrix> localTransform,
        //              RefRW<Health> health)
        //          in SystemAPI.Query<
        //              RefRW<PostTransformMatrix>,
        //              RefRW<Health>>())
        foreach ((RefRW<Health> health, Entity entity)
                 in SystemAPI.Query<RefRW<Health>>().WithEntityAccess())
        {
            if (health.ValueRW.onHealthChanged)
            {
                health.ValueRW.onHealthChanged = false;
                float healthNormalized = health.ValueRW.healthAmount / (float)health.ValueRW.healthAmountMax;
                //Debug.Log(health.faction + ":" + health.healthAmount);
                //localTransform.ValueRW.Value = float4x4.Scale(healthNormalized, 1, 1);

                Debug.Log("Health:" + health.ValueRW.healthAmount);
                
                //主城 发送数据到UI层
                if (health.ValueRO.IsCity)
                {
                    _writer.Enqueue(new RcvData(health.ValueRW.faction, 1, healthNormalized));
                }

                //死亡判定
                if (health.ValueRW.healthAmount <= 0)
                {
                    EntityCommandBuffer entityCommandBuffer =
                        SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                            .CreateCommandBuffer(state.WorldUnmanaged);
                    entityCommandBuffer.DestroyEntity(entity);
                }
            }
        }
    }
}