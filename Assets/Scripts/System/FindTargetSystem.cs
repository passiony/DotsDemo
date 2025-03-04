using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

namespace System
{
    public partial struct FindTargetSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            PhysicsWorldSingleton physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
            CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;
            NativeList<DistanceHit> distanceHitList = new NativeList<DistanceHit>(Allocator.Temp);

            foreach ((RefRO<LocalTransform> localTransform,
                         RefRW<FindTarget> findTarget,
                         RefRW<UnitMover> mover)
                     in SystemAPI.Query<
                         RefRO<LocalTransform>,
                         RefRW<FindTarget>,RefRW<UnitMover>>()) {

                findTarget.ValueRW.timer -= SystemAPI.Time.DeltaTime;
                if (findTarget.ValueRO.timer > 0f) {
                    // Timer not elapsed
                    continue;
                }
                findTarget.ValueRW.timer = findTarget.ValueRO.timerMax;
                
                distanceHitList.Clear();
                CollisionFilter collisionFilter = CollisionFilter.Default;
                
                if (collisionWorld.OverlapSphere(localTransform.ValueRO.Position, findTarget.ValueRO.range, ref distanceHitList, collisionFilter)) {
                    foreach (DistanceHit distanceHit in distanceHitList) {
                        if (!SystemAPI.Exists(distanceHit.Entity) || !SystemAPI.HasComponent<Unit>(distanceHit.Entity)) {
                            continue;
                        }
                        Unit targetUnit = SystemAPI.GetComponent<Unit>(distanceHit.Entity);
                        if (targetUnit.faction == findTarget.ValueRO.targetFaction) {
                            // Valid target
                            var targetTransform = SystemAPI.GetComponent<LocalTransform>(distanceHit.Entity);
                            mover.ValueRW.targetPosition = targetTransform.Position;
                            break;
                        }
                    }
                }
            }
        }
    }
}