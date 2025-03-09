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
        var job = new GoldJob
        {
            Writer = _writer
        };

        var jobHandle = job.ScheduleParallel(state.Dependency);
        jobHandle.Complete();
    }
}

[BurstCompile]
public partial struct GoldJob : IJobEntity
{
    public NativeQueue<RcvData>.ParallelWriter Writer;

    public void Execute(ref Gold gold)
    {
        if (gold.onGoldChanged)
        {
            gold.onGoldChanged = false;
            
            // 发送数据到UI层
            Writer.Enqueue(new RcvData(gold.faction, 2, gold.goldAmount));
        }
    }
}