using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

partial struct ShootAttackSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        state.RequireForUpdate<EntitiesReferences>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var job = new ShootAttackJob
        {
            EntitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>(),
            DeltaTime = SystemAPI.Time.DeltaTime,
            ECB = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter()
        };

        var jobHandle = job.ScheduleParallel(state.Dependency);
        jobHandle.Complete();
    }
}

[BurstCompile]
public partial struct ShootAttackJob : IJobEntity
{
    public EntitiesReferences EntitiesReferences;
    public float DeltaTime;
    public EntityCommandBuffer.ParallelWriter ECB;

    public void Execute([EntityIndexInQuery] int index,
        ref LocalTransform localTransform,
        ref FindTarget findTarget,
        ref ShootAttack shootAttack,
        ref UnitMover unitMover)
    {
        float3 aimDirection = unitMover.targetPosition - localTransform.Position;
        aimDirection = math.normalize(aimDirection);

        quaternion targetRotation = quaternion.LookRotation(aimDirection, math.up());
        localTransform.Rotation =
            math.slerp(localTransform.Rotation, targetRotation, DeltaTime * unitMover.rotationSpeed);

        shootAttack.timer -= DeltaTime;
        if (shootAttack.timer > 0f)
        {
            return;
        }

        shootAttack.timer = shootAttack.timerMax;

        Entity bulletEntity = ECB.Instantiate(index, EntitiesReferences.bulletPrefabEntity);
        float3 bulletSpawnWorldPosition = localTransform.TransformPoint(shootAttack.bulletSpawnLocalPosition);
        ECB.SetComponent(index, bulletEntity, LocalTransform.FromPosition(bulletSpawnWorldPosition));

        ECB.SetComponent(index, bulletEntity, new Bullet
        {
            speed = 5,
            damageAmount = shootAttack.damageAmount
        });

        ECB.SetComponent(index, bulletEntity, new Target
        {
            targetEntity = findTarget.targetEntity
        });
        shootAttack.onShoot.isTriggered = true;
        shootAttack.onShoot.shootFromPosition = bulletSpawnWorldPosition;
    }
}