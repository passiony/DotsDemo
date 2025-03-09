using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

partial struct BulletMoverSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

        // 在System的OnUpdate中：
        var transformLookup = SystemAPI.GetComponentLookup<LocalTransform>();
        var victimLookup = SystemAPI.GetComponentLookup<ShootVictim>(true);
        var healthLookup = SystemAPI.GetComponentLookup<Health>();

        var job = new BulletMoverJob
        {
            TransformLookup = transformLookup,
            VictimLookup = victimLookup,
            HealthLookup = healthLookup,
            deltaTime = SystemAPI.Time.DeltaTime,
            ECB = ecb,
        };

        var jobHandle = job.ScheduleParallel(state.Dependency);
        jobHandle.Complete();
    }
}

[BurstCompile]
public partial struct BulletMoverJob : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter ECB;
    public float deltaTime;

    [NativeDisableParallelForRestriction] public ComponentLookup<LocalTransform> TransformLookup;

    [NativeDisableParallelForRestriction, ReadOnly]
    public ComponentLookup<ShootVictim> VictimLookup;

    [NativeDisableParallelForRestriction] public ComponentLookup<Health> HealthLookup;

    public void Execute([EntityIndexInQuery] int index,
        in Bullet bullet,
        in Target target,
        Entity entity)
    {
        // 目标实体检查
        if (target.targetEntity == Entity.Null ||
            !VictimLookup.HasComponent(target.targetEntity))
        {
            ECB.DestroyEntity(index, entity);
            return;
        }

        var localTransform = TransformLookup.GetRefRW(entity);
        var targetLocalTransform = TransformLookup.GetRefRW(target.targetEntity);
        var targetShootVictim = VictimLookup.GetRefRO(target.targetEntity);

        float3 targetPosition = targetLocalTransform.ValueRW.TransformPoint(targetShootVictim.ValueRO.hitLocalPosition);
        float distanceBeforeSq = math.distancesq(localTransform.ValueRO.Position, targetPosition);
        float3 moveDirection = targetPosition - localTransform.ValueRO.Position;
        moveDirection = math.normalize(moveDirection);

        localTransform.ValueRW.Position += moveDirection * bullet.speed * deltaTime;
        float distanceAfterSq = math.distancesq(localTransform.ValueRO.Position, targetPosition);

        if (distanceAfterSq > distanceBeforeSq)
        {
            // Overshot
            localTransform.ValueRW.Position = targetPosition;
        }

        float destroyDistanceSq = .2f;
        if (math.distancesq(localTransform.ValueRO.Position, targetPosition) < destroyDistanceSq)
        {
            // Close enough to damage target
            if (HealthLookup.TryGetComponent(target.targetEntity, out Health health))
            {
                health.healthAmount -= bullet.damageAmount;
                health.onHealthChanged = true;
                HealthLookup[target.targetEntity] = health;
            }

            ECB.DestroyEntity(index, entity);
        }
    }
}