using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

namespace System
{
    public partial struct FindTargetSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PhysicsWorldSingleton>();
        }

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
                    Entity closestTarget = Entity.Null;
                    float closestDistanceSq = float.MaxValue;

                    //寻找最近的目标
                    foreach (DistanceHit distanceHit in distanceHitList) {
                        if (!SystemAPI.Exists(distanceHit.Entity) || !SystemAPI.HasComponent<Unit>(distanceHit.Entity)) {
                            continue;
                        }
                        Unit targetUnit = SystemAPI.GetComponent<Unit>(distanceHit.Entity);
                        if (targetUnit.faction == findTarget.ValueRO.targetFaction) {
                            // Valid target
                            float distanceSq = math.distancesq(localTransform.ValueRO.Position, distanceHit.Position);
                            if (distanceSq < closestDistanceSq) {
                                closestTarget = distanceHit.Entity;
                                closestDistanceSq = distanceSq;
                            }
                        }
                    }

                    if (closestTarget != Entity.Null) {
                        var targetTransform = SystemAPI.GetComponent<LocalTransform>(closestTarget);
                        mover.ValueRW.targetPosition = targetTransform.Position;
                        findTarget.ValueRW.targetEntity = closestTarget;
                        Debug.Log("找到最近目标：" + SystemAPI.GetComponent<Unit>(closestTarget).Name);
                    }
                }
            }
        }
    }
}