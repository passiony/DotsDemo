using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using RaycastHit = Unity.Physics.RaycastHit;

partial struct MeleeAttackSystem : ISystem
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
        NativeList<RaycastHit> raycastHitList = new NativeList<RaycastHit>(Allocator.Temp);

        foreach ((RefRO<LocalTransform> localTransform,
                     RefRW<MeleeAttack> meleeAttack,
                     RefRW<UnitMover> unitMover,
                     RefRW<Unit> unit,
                     Entity entity)
                 in SystemAPI.Query<
                     RefRO<LocalTransform>,
                     RefRW<MeleeAttack>,
                     RefRW<UnitMover>,
                     RefRW<Unit>>().WithEntityAccess())
        {
            //攻击间隔
            meleeAttack.ValueRW.timer -= SystemAPI.Time.DeltaTime;
            if (meleeAttack.ValueRO.timer > 0) {
                continue;
            }
            meleeAttack.ValueRW.timer = meleeAttack.ValueRO.timerMax;
            
            //攻击逻辑
            Debug.Log("攻击一次");
            float meleeAttackDistanceSq = 10f;
            bool isCloseEnoughToAttack =
                math.distancesq(localTransform.ValueRO.Position, unitMover.ValueRO.targetPosition) <
                meleeAttackDistanceSq;
            
            if (isCloseEnoughToAttack)
            {
                float3 dirToTarget = unitMover.ValueRO.targetPosition - localTransform.ValueRO.Position;
                dirToTarget = math.normalize(dirToTarget);
                float distanceExtraToTestRaycast = 4f;
                RaycastInput raycastInput = new RaycastInput
                {
                    Start = localTransform.ValueRO.Position,
                    End = localTransform.ValueRO.Position +
                          dirToTarget * (meleeAttack.ValueRO.colliderSize + distanceExtraToTestRaycast),
                    Filter = CollisionFilter.Default,
                };
                raycastHitList.Clear();
                if (collisionWorld.CastRay(raycastInput, ref raycastHitList))
                {
                    foreach (RaycastHit raycastHit in raycastHitList)
                    {
                        if (!SystemAPI.HasComponent<Health>(raycastHit.Entity))
                        {
                            continue;
                        }
                        RefRW<Health> targetHealth = SystemAPI.GetComponentRW<Health>(raycastHit.Entity);
                        if (targetHealth.ValueRW.faction == unit.ValueRW.faction)
                        {
                            continue;
                        }
                        targetHealth.ValueRW.healthAmount -= meleeAttack.ValueRO.damageAmount;
                        Debug.Log("攻击到目标：" + targetHealth.ValueRW.healthAmount);
                        targetHealth.ValueRW.onHealthChanged = true;
                        break;
                    }
                }
            }
        }
    }
}