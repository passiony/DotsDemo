using System;
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
            AddSpawnCommand(Faction.Red, SolderType.Melee);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            AddSpawnCommand(Faction.Red, SolderType.Shooting);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            AddSpawnCommand(Faction.Red, SolderType.Mining);
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            AddSpawnCommand(Faction.Blue, SolderType.Melee);
        }

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            AddSpawnCommand(Faction.Blue, SolderType.Shooting);
        }

        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            AddSpawnCommand(Faction.Blue, SolderType.Mining);
        }

        QueryDots();
    }

    void AddSpawnCommand(Faction faction, SolderType solder, int num = 1)
    {
        DynamicBuffer<GameCommand> buffer =
            _entityManager.GetBuffer<GameCommand>(_commandEntity);

        // 解析网络数据为命令（示例）
        var command = new GameCommand
        {
            CMD = CommandType.SpawnUnit,
            Faction = faction,
            Solder = solder,
            Number = num
        };
        buffer.Add(command);
    }

    void QueryDots()
    {
        while (Facade.SharedQueue.TryDequeue(out RcvData value))
        {
            // 主线程安全读取
            switch (value.type)
            {
                case 1:
                    UIManager.Instance.SetHP(value.faction, value.value);
                    break;
                case 2:
                    UIManager.Instance.SetMoney(value.faction, (int)value.value);
                    break;
            }
        }
    }

    private void OnDestroy()
    {
        Facade.Cleanup();
    }
}