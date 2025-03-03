using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private EntityManager _entityManager;
    private Entity _commandEntity;

    private void Start()
    {
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        //创建一个专门用于存储游戏命令的"信箱实体"，相当于在 ECS 世界中创建了一个命令接收站
        _commandEntity = _entityManager.CreateEntity();
        // 为该实体附加一个可动态增长的缓冲区，用于存储多个 GameCommand 命令
        _entityManager.AddBuffer<GameCommand>(_commandEntity);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            AddSpawnCommand(Faction.Red);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            AddSpawnCommand(Faction.Blue);
        }
    }

    void AddSpawnCommand(Faction faction)
    {
        DynamicBuffer<GameCommand> buffer =
            _entityManager.GetBuffer<GameCommand>(_commandEntity);

        // 解析网络数据为命令（示例）
        var command = new GameCommand
        {
            Type = GameCommand.CommandType.SpawnUnit,
            Faction = faction,
        };
        buffer.Add(command);
    }
}