using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial struct HealthSystem : ISystem
{
    private NativeQueue<RcvData>.ParallelWriter _writer;
    private EntityCommandBuffer.ParallelWriter _ecb;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        _writer = Facade.SharedQueue.AsParallelWriter();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        _ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
        var job = new HealthJob
        {
            Writer = _writer,
            ECB = _ecb,
        };
        var jobHandle = job.ScheduleParallel(state.Dependency);
        jobHandle.Complete(); // 确保Job执行完成
    }
}

[BurstCompile]
public partial struct HealthJob : IJobEntity
{
    public NativeQueue<RcvData>.ParallelWriter Writer;
    public EntityCommandBuffer.ParallelWriter ECB;

    public void Execute([EntityIndexInQuery] int index,
        ref Health health,
        Entity entity)
    {
        float healthNormalized = health.healthAmount / (float)health.healthAmountMax;
        //主城 发送数据到UI层
        if (health.IsCity)
        {
            Writer.Enqueue(new RcvData(health.faction, 1, healthNormalized));
        }

        //死亡判定
        if (health.healthAmount <= 0)
        {
            // Debug.Log("角色死亡");
            ECB.DestroyEntity(index, entity);
        }
    }
}