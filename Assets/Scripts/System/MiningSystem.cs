using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

partial struct MiningSystem : ISystem
{
    private ComponentLookup<Gold> goldLookup;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PhysicsWorldSingleton>();
        goldLookup = state.GetComponentLookup<Gold>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        PhysicsWorldSingleton physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        goldLookup.Update(ref state); // 更新ComponentLookup以确保它在Job执行期间是有效的

        var job = new MiningJob
        {
            GoldLookup = goldLookup,
            PhysicsWorldSingleton = physicsWorldSingleton,
            DeltaTime = SystemAPI.Time.DeltaTime
        };

        var jobHandle = job.ScheduleParallel(state.Dependency);
        jobHandle.Complete();
    }
}

[BurstCompile]
partial struct MiningJob : IJobEntity
{
    [ReadOnly] public PhysicsWorldSingleton PhysicsWorldSingleton; 
    public float DeltaTime;
    [NativeDisableParallelForRestriction] public ComponentLookup<Gold> GoldLookup;

    public void Execute(ref Mining mining, ref UnitMover unitMover, in LocalTransform localTransform)
    {
        // 攻击间隔
        mining.timer -= DeltaTime;
        if (mining.timer > 0)
        {
            return;
        }

        mining.timer = mining.timerMax;

        // 开采矿石
        int distance = 2;
        bool isCloseEnoughToAttack = math.distancesq(localTransform.Position, unitMover.targetPosition) < distance;
        if (isCloseEnoughToAttack)
        {
            NativeList<DistanceHit> distanceHitList = new NativeList<DistanceHit>(Allocator.Temp);
            CollisionFilter collisionFilter = CollisionFilter.Default;

            if (PhysicsWorldSingleton.CollisionWorld.OverlapSphere(localTransform.Position, 2, ref distanceHitList,
                    collisionFilter))
            {
                foreach (var raycastHit in distanceHitList)
                {
                    if (!GoldLookup.HasComponent(raycastHit.Entity))
                    {
                        continue;
                    }

                    RefRW<Gold> targetHealth = GoldLookup.GetRefRW(raycastHit.Entity); // 使用ComponentLookup获取Gold组件
                    targetHealth.ValueRW.goldAmount = mining.momeySpeed;
                    targetHealth.ValueRW.onGoldChanged = true;
                    break;
                }
            }

            distanceHitList.Dispose();
        }
    }
}