using System;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public partial struct UnitSpawnerSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        state.RequireForUpdate<EntitiesReferences>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
        var entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();
        var job = new UnitSpawnerJob
        {
            EntitiesReferences = entitiesReferences,
            ECB = ecb
        };

        var jobHandle = job.ScheduleParallel(state.Dependency);
        jobHandle.Complete();
    }
}

[BurstCompile]
public partial struct UnitSpawnerJob : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter ECB;
    public EntitiesReferences EntitiesReferences;

    public void Execute([EntityIndexInQuery] int index, ref DynamicBuffer<GameCommand> buffer)
    {
        foreach (GameCommand cmd in buffer)
        {
            ProcessCommand(index, cmd);
        }

        buffer.Clear();
    }

    private void ProcessCommand(int index, GameCommand cmd)
    {
        switch (cmd.CMD)
        {
            case CommandType.SpawnUnit:
                var prefabEntity = GetPrefab(cmd);
                float3 startPosition = GetBornPos(cmd);
                float3 targetPosition = GetTargetPos(cmd);

                Entity zombieEntity = ECB.Instantiate(index, prefabEntity);
                ECB.SetComponent(index, zombieEntity, LocalTransform.FromPosition(startPosition));
                ECB.SetComponent(index, zombieEntity, new UnitMover
                {
                    moveSpeed = 2,
                    rotationSpeed = 2,
                    targetPosition = targetPosition
                });
                break;
            case CommandType.DestroyUnit:
                break;
            case CommandType.ChangeTeam:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    Entity GetPrefab(GameCommand cmd)
    {
        switch (cmd.Faction)
        {
            case Faction.Red:
                switch (cmd.Solder)
                {
                    case SolderType.Melee:
                        return EntitiesReferences.redMeleePrefabEntity;
                    case SolderType.Shooting:
                        return EntitiesReferences.redShootPrefabEntity;
                    case SolderType.Mining:
                        return EntitiesReferences.redMiningPrefabEntity;
                }

                break;
            case Faction.Blue:
                switch (cmd.Solder)
                {
                    case SolderType.Melee:
                        return EntitiesReferences.blueMeleePrefabEntity;
                    case SolderType.Shooting:
                        return EntitiesReferences.blueShootPrefabEntity;
                    case SolderType.Mining:
                        return EntitiesReferences.blueMiningPrefabEntity;
                }

                break;
        }

        return Entity.Null;
    }

    float3 GetBornPos(GameCommand cmd)
    {
        switch (cmd.Faction)
        {
            case Faction.Red:
                return EntitiesReferences.redStartPosition;
            case Faction.Blue:
                return EntitiesReferences.blueStartPosition;
        }

        return float3.zero;
    }

    float3 GetTargetPos(GameCommand cmd)
    {
        switch (cmd.Faction)
        {
            case Faction.Red:
                switch (cmd.Solder)
                {
                    case SolderType.Melee:
                    case SolderType.Shooting:
                        return EntitiesReferences.blueStartPosition;
                    case SolderType.Mining:
                        return EntitiesReferences.redMinePosition;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                break;
            case Faction.Blue:
                switch (cmd.Solder)
                {
                    case SolderType.Melee:
                    case SolderType.Shooting:
                        return EntitiesReferences.redStartPosition;
                    case SolderType.Mining:
                        return EntitiesReferences.blueMinePosition;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                break;
        }

        return float3.zero;
    }
}