using System;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial struct UnitSpawnerSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntitiesReferences>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var buffer in SystemAPI.Query<DynamicBuffer<GameCommand>>())
        {
            foreach (GameCommand cmd in buffer)
            {
                ProcessCommand(ref state, cmd);
            }

            buffer.Clear();
        }
    }

    [BurstCompile]
    private void ProcessCommand(ref SystemState state, GameCommand cmd)
    {
        switch (cmd.CMD)
        {
            case CommandType.SpawnUnit:
                var prefabEntity = GetPrefab(cmd);
                float3 startPosition = GetBornPos(cmd);
                float3 targetPosition = GetTargetPos(cmd);
                
                Entity zombieEntity = state.EntityManager.Instantiate(prefabEntity);
                SystemAPI.SetComponent(zombieEntity, LocalTransform.FromPosition(startPosition));
                var mover = SystemAPI.GetComponentRW<UnitMover>(zombieEntity);
                mover.ValueRW.targetPosition = targetPosition;
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
        EntitiesReferences entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();
        switch (cmd.Faction)
        {
            case Faction.Red:
                switch (cmd.Solder)
                {
                    case SolderType.Melee:
                        return entitiesReferences.redMeleePrefabEntity;
                    case SolderType.Shooting:
                        return entitiesReferences.redShootPrefabEntity;
                    case SolderType.Mining:
                        return entitiesReferences.redMiningPrefabEntity;
                }
                break;
            case Faction.Blue:
                switch (cmd.Solder)
                {
                    case SolderType.Melee:
                        return entitiesReferences.blueMeleePrefabEntity;
                    case SolderType.Shooting:
                        return entitiesReferences.blueShootPrefabEntity;
                    case SolderType.Mining:
                        return entitiesReferences.blueMiningPrefabEntity;
                }
                break;
        }
        return Entity.Null;
    }
    float3 GetBornPos(GameCommand cmd)
    {
        EntitiesReferences entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();
        switch (cmd.Faction)
        {
            case Faction.Red:
                return entitiesReferences.redStartPosition;
            case Faction.Blue:
                return entitiesReferences.blueStartPosition;
        }
        return float3.zero;
    }
    float3 GetTargetPos(GameCommand cmd)
    {
        EntitiesReferences entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();
        switch (cmd.Faction)
        {
            case Faction.Red:
                switch (cmd.Solder)
                {
                    case SolderType.Melee:
                    case SolderType.Shooting:
                        return entitiesReferences.blueStartPosition;
                    case SolderType.Mining:
                        return entitiesReferences.redMinePosition;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                break;
            case Faction.Blue:
                switch (cmd.Solder)
                {
                    case SolderType.Melee:
                    case SolderType.Shooting:
                        return entitiesReferences.redStartPosition;
                    case SolderType.Mining:
                        return entitiesReferences.blueStartPosition;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                break;
        }
        return float3.zero;
    }
}