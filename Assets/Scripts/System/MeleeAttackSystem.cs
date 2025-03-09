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

        var job = new MeleeAttackJob
        {
            CollisionWorld = collisionWorld,
            DeltaTime = SystemAPI.Time.DeltaTime,
            HealthLookup = SystemAPI.GetComponentLookup<Health>() // 标记为只读
        };

        var jobHandle = job.ScheduleParallel(state.Dependency);
        jobHandle.Complete();
    }
}

[BurstCompile]
public partial struct MeleeAttackJob : IJobEntity
{
    [ReadOnly]
    public CollisionWorld CollisionWorld;
    public float DeltaTime;
    [NativeDisableParallelForRestriction]
    public ComponentLookup<Health> HealthLookup; // 使用ComponentLookup来获取Health组件

    public void Execute(Entity entity,
        ref MeleeAttack meleeAttack,
        ref UnitMover unitMover,
        ref Unit unit,
        in LocalTransform localTransform)
    {
        // 攻击间隔
        meleeAttack.timer -= DeltaTime;
        if (meleeAttack.timer > 0)
        {
            return;
        }

        meleeAttack.timer = meleeAttack.timerMax;

        // 攻击逻辑
        // Debug.Log("攻击一次");
        float meleeAttackDistanceSq = 10f;
        bool isCloseEnoughToAttack =
            math.distancesq(localTransform.Position, unitMover.targetPosition) <
            meleeAttackDistanceSq;

        if (isCloseEnoughToAttack)
        {
            float3 dirToTarget = unitMover.targetPosition - localTransform.Position;
            dirToTarget = math.normalize(dirToTarget);
            float distanceExtraToTestRaycast = 4f;
            RaycastInput raycastInput = new RaycastInput
            {
                Start = localTransform.Position,
                End = localTransform.Position +
                      dirToTarget * (meleeAttack.colliderSize + distanceExtraToTestRaycast),
                Filter = CollisionFilter.Default,
            };
            var RaycastHitList = new NativeList<RaycastHit>(Allocator.TempJob);
            if (CollisionWorld.CastRay(raycastInput, ref RaycastHitList))
            {
                foreach (RaycastHit raycastHit in RaycastHitList)
                {
                    if (!HealthLookup.HasComponent(raycastHit.Entity))
                    {
                        continue;
                    }

                    var targetHealth = HealthLookup.GetRefRW(raycastHit.Entity); // 使用ComponentLookup获取Health组件
                    if (targetHealth.ValueRO.healthAmount <= 0)
                    {
                        targetHealth.ValueRW.onHealthChanged = true;
                        continue;
                    }

                    if (targetHealth.ValueRO.faction == unit.faction)
                    {
                        continue;
                    }

                    targetHealth.ValueRW.healthAmount -= meleeAttack.damageAmount;
                    // Debug.Log("攻击到目标：" + targetHealth.ValueRO.healthAmount);
                    targetHealth.ValueRW.onHealthChanged = true;
                    break;
                }
            }
        }
    }
}