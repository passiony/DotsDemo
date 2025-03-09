using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

public partial struct FindTargetSystem : ISystem
{
    private ComponentLookup<Unit> unitLookup;
    private ComponentLookup<LocalTransform> LocalTransformLookup;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PhysicsWorldSingleton>();
        unitLookup = state.GetComponentLookup<Unit>();
        LocalTransformLookup = state.GetComponentLookup<LocalTransform>(true);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        PhysicsWorldSingleton physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        unitLookup.Update(ref state);
        LocalTransformLookup.Update(ref state);

        var job = new FindTargetJob
        {
            PhysicsWorldSingleton = physicsWorldSingleton,
            DeltaTime = SystemAPI.Time.DeltaTime,
            UnitLookup = unitLookup,
            LocalTransformLookup = LocalTransformLookup,
        };

        var jobHandle = job.ScheduleParallel(state.Dependency);
        jobHandle.Complete();
    }
}

[BurstCompile]
public partial struct FindTargetJob : IJobEntity
{
    [ReadOnly] public PhysicsWorldSingleton PhysicsWorldSingleton;
    public float DeltaTime;
    [NativeDisableParallelForRestriction] public ComponentLookup<Unit> UnitLookup;
    [ReadOnly] public ComponentLookup<LocalTransform> LocalTransformLookup; 

    public void Execute(ref FindTarget findTarget, ref UnitMover mover, in LocalTransform localTransform)
    {
        findTarget.timer -= DeltaTime;
        if (findTarget.timer > 0f)
        {
            // Timer not elapsed
            return;
        }

        findTarget.timer = findTarget.timerMax;

        NativeList<DistanceHit> distanceHitList = new NativeList<DistanceHit>(Allocator.Temp);
        CollisionFilter collisionFilter = CollisionFilter.Default;

        if (PhysicsWorldSingleton.CollisionWorld.OverlapSphere(localTransform.Position, findTarget.range,
                ref distanceHitList, collisionFilter))
        {
            Entity closestTarget = Entity.Null;
            float closestDistanceSq = float.MaxValue;

            //寻找最近的目标
            foreach (DistanceHit distanceHit in distanceHitList)
            {
                if (!UnitLookup.HasComponent(distanceHit.Entity)) // 使用ComponentLookup检查实体是否存在
                {
                    continue;
                }

                Unit targetUnit = UnitLookup[distanceHit.Entity];
                if (targetUnit.faction == findTarget.targetFaction)
                {
                    // Valid target
                    float distanceSq = math.distancesq(localTransform.Position, distanceHit.Position);
                    if (distanceSq < closestDistanceSq)
                    {
                        closestTarget = distanceHit.Entity;
                        closestDistanceSq = distanceSq;
                    }
                }
            }

            if (closestTarget != Entity.Null)
            {
                var targetTransform = LocalTransformLookup[closestTarget]; // 使用ComponentLookup获取LocalTransform组件
                mover.targetPosition = targetTransform.Position;
                findTarget.targetEntity = closestTarget;
            }
        }

        distanceHitList.Dispose();
    }
}