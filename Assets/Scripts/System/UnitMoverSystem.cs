using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

namespace System
{
    public partial struct UnitMoverSystem : ISystem
    {
        public const float REACHED_TARGET_POSITION_DISTANCE_SQ = 10f;

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach ((RefRW<LocalTransform> localTransform,
                         RefRO<UnitMover> unitMover,
                         RefRW<PhysicsVelocity> physicsVelocity)
                     in SystemAPI.Query<
                         RefRW<LocalTransform>,
                         RefRO<UnitMover>,
                         RefRW<PhysicsVelocity>>())
            {
                float3 moveDirection = unitMover.ValueRO.targetPosition - localTransform.ValueRO.Position;
                moveDirection.y = 0f;

                // Reached the target position
                var lengthsq = math.lengthsq(moveDirection);
                float reachedTargetDistanceSq = UnitMoverSystem.REACHED_TARGET_POSITION_DISTANCE_SQ;
                if (lengthsq <= reachedTargetDistanceSq)
                {
                    // 保持当前速度的X和Z分量，仅将Y轴分量设置为0
                    physicsVelocity.ValueRW.Linear.y = 0f;
                    physicsVelocity.ValueRW.Linear.x = physicsVelocity.ValueRO.Linear.x;
                    physicsVelocity.ValueRW.Linear.z = physicsVelocity.ValueRO.Linear.z;
                    physicsVelocity.ValueRW.Angular = float3.zero;
                    continue;
                }
                
                moveDirection = math.normalize(moveDirection);
                // 计算 Y 轴的旋转角度
                float targetYRotation = math.atan2(moveDirection.x, moveDirection.z);
                quaternion targetRotation = quaternion.RotateY(targetYRotation);

                localTransform.ValueRW.Rotation =
                    math.slerp(localTransform.ValueRO.Rotation,
                        targetRotation,
                        SystemAPI.Time.DeltaTime * unitMover.ValueRO.rotationSpeed);

                physicsVelocity.ValueRW.Linear = moveDirection * unitMover.ValueRO.moveSpeed;
                physicsVelocity.ValueRW.Angular = float3.zero;
            }
        }
    }
}