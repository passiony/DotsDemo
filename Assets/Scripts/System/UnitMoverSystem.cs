using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

public partial struct UnitMoverSystem : ISystem
{
    public const float REACHED_TARGET_POSITION_DISTANCE_SQ = 10f;

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        new UnitMoverJob
        {
            deltaTime = SystemAPI.Time.DeltaTime,
        }.ScheduleParallel();
    }
}

[BurstCompile]
public partial struct UnitMoverJob : IJobEntity
{
    public float deltaTime;

    public void Execute(ref LocalTransform localTransform, in UnitMover unitMover,
        ref PhysicsVelocity physicsVelocity)
    {
        float3 moveDirection = unitMover.targetPosition - localTransform.Position;
        moveDirection.y = 0f;

        // Reached the target position
        var lengthsq = math.lengthsq(moveDirection);
        float reachedTargetDistanceSq = UnitMoverSystem.REACHED_TARGET_POSITION_DISTANCE_SQ;
        if (lengthsq <= reachedTargetDistanceSq)
        {
            // 保持当前速度的X和Z分量，仅将Y轴分量设置为0
            physicsVelocity.Linear.y = 0f;
            physicsVelocity.Linear.x = physicsVelocity.Linear.x;
            physicsVelocity.Linear.z = physicsVelocity.Linear.z;
            physicsVelocity.Angular = float3.zero;
            return;
        }

        moveDirection = math.normalize(moveDirection);
        // 计算 Y 轴的旋转角度
        float targetYRotation = math.atan2(moveDirection.x, moveDirection.z);
        quaternion targetRotation = quaternion.RotateY(targetYRotation);

        localTransform.Rotation =
            math.slerp(localTransform.Rotation,
                targetRotation,
                deltaTime * unitMover.rotationSpeed);

        physicsVelocity.Linear = moveDirection * unitMover.moveSpeed;
        physicsVelocity.Angular = float3.zero;
    }
}
