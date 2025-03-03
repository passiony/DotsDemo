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
        switch (cmd.Type)
        {
            case GameCommand.CommandType.SpawnUnit:
                EntitiesReferences entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();
                var prefabEntity = cmd.Faction == Faction.Red
                    ? entitiesReferences.redSoldierPrefabEntity
                    : entitiesReferences.blueSoldierPrefabEntity;
                var startPosition = cmd.Faction == Faction.Red
                    ? entitiesReferences.redStartPosition
                    : entitiesReferences.blueStartPosition;
                var targetPosition = cmd.Faction == Faction.Red
                ? entitiesReferences.blueStartPosition
                : entitiesReferences.redStartPosition;

                Entity zombieEntity = state.EntityManager.Instantiate(prefabEntity);
                SystemAPI.SetComponent(zombieEntity, LocalTransform.FromPosition(startPosition));
                var mover = SystemAPI.GetComponentRW<UnitMover>(zombieEntity);
                mover.ValueRW.targetPosition = targetPosition;
                break;
            case GameCommand.CommandType.DestroyUnit:
                break;
            case GameCommand.CommandType.ChangeTeam:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}