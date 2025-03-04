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
        NativeList<RaycastHit> raycastHitList = new NativeList<RaycastHit>(Allocator.Temp);

        foreach ((RefRO<LocalTransform> localTransform,
                     RefRW<Mining> mining,
                     RefRW<UnitMover> unitMover,
                     RefRW<Unit> unit,
                     Entity entity)
                 in SystemAPI.Query<
                     RefRO<LocalTransform>,
                     RefRW<Mining>,
                     RefRW<UnitMover>,
                     RefRW<Unit>>().WithEntityAccess())
        {
            float meleeAttackDistanceSq = 2f;
            bool isCloseEnoughToAttack =
                math.distancesq(localTransform.ValueRO.Position, unitMover.ValueRO.targetPosition) <
                meleeAttackDistanceSq;
            EntityCommandBuffer entityCommandBuffer =
                SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

            if (!isCloseEnoughToAttack)
            {
                float3 dirToTarget = unitMover.ValueRO.targetPosition - localTransform.ValueRO.Position;
                dirToTarget = math.normalize(dirToTarget);
                float distanceExtraToTestRaycast = 2f;
                RaycastInput raycastInput = new RaycastInput
                {
                    Start = localTransform.ValueRO.Position,
                    End = localTransform.ValueRO.Position +
                          dirToTarget * (mining.ValueRO.colliderSize + distanceExtraToTestRaycast),
                    Filter = CollisionFilter.Default,
                };
                raycastHitList.Clear();
                if (collisionWorld.CastRay(raycastInput, ref raycastHitList))
                {
                    foreach (RaycastHit raycastHit in raycastHitList)
                    {
                        if (!SystemAPI.HasComponent<Gold>(raycastHit.Entity))
                        {
                            continue;
                        }
                        RefRW<Gold> targetHealth = SystemAPI.GetComponentRW<Gold>(raycastHit.Entity);
                        if (targetHealth.ValueRW.faction == unit.ValueRW.faction)
                        {
                            continue;
                        }
                        targetHealth.ValueRW.goldAmount += mining.ValueRO.momeySpeed;
                        targetHealth.ValueRW.onGoldChanged = true;
                        
                        entityCommandBuffer.DestroyEntity(entity);
                        break;
                    }
                }
            }
        }
    }
}