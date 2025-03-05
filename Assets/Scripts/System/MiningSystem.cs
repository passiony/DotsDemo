using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

partial struct MiningSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        PhysicsWorldSingleton physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;
        NativeList<DistanceHit> distanceHitList = new NativeList<DistanceHit>(Allocator.Temp);

        foreach ((RefRO<LocalTransform> localTransform,
                     RefRW<Mining> mining,
                     RefRW<UnitMover> unitMover)
                 in SystemAPI.Query<
                     RefRO<LocalTransform>,
                     RefRW<Mining>,
                     RefRW<UnitMover>>())
        {
            //攻击间隔
            mining.ValueRW.timer -= SystemAPI.Time.DeltaTime;
            if (mining.ValueRO.timer > 0) {
                continue;
            }
            mining.ValueRW.timer = mining.ValueRO.timerMax;

            //开采矿石
            bool isCloseEnoughToAttack = math.distancesq(localTransform.ValueRO.Position, unitMover.ValueRO.targetPosition) < 10;
            if (isCloseEnoughToAttack)
            {
                distanceHitList.Clear();
                CollisionFilter collisionFilter = CollisionFilter.Default;

                if (collisionWorld.OverlapSphere(localTransform.ValueRO.Position, 2, ref distanceHitList,
                        collisionFilter))
                {
                    foreach (var raycastHit in distanceHitList)
                    {
                        if (!SystemAPI.HasComponent<Gold>(raycastHit.Entity))
                        {
                            continue;
                        }
                        RefRW<Gold> targetHealth = SystemAPI.GetComponentRW<Gold>(raycastHit.Entity);
                        targetHealth.ValueRW.goldAmount += mining.ValueRO.momeySpeed;
                        targetHealth.ValueRW.onGoldChanged = true;
                        break;
                    }
                }
            }
        }
    }
}