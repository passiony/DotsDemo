using System;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    private EntityManager _entityManager;
    private Entity _commandEntity;
    public int Money = 100;
    public int Cost = 2;
    public bool GameOver;

    private void Start()
    {
        Instance = this;
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        //创建一个专门用于存储游戏命令的"信箱实体"，相当于在 ECS 世界中创建了一个命令接收站
        _commandEntity = _entityManager.CreateEntity();
        // 为该实体附加一个可动态增长的缓冲区，用于存储多个 GameCommand 命令
        _entityManager.AddBuffer<GameCommand>(_commandEntity);

        Money = 100;
        GameOver = false;
    }

    bool UseMoney(int num)
    {
        if (Money > num)
        {
            Money -= num;
            return true;
        }
        return false;
    }

    public void Gen(Faction faction, SolderType solder, int num = 1)
    {
        if (UseMoney(Cost*num))
        {
            for (int i = 0; i < num; i++)
            {
                AddSpawnCommand(faction, solder);
                UIManager.Instance.SetMoney(Faction.Red, Money);
            }
        }
    }
    
    private void Update()
    {
        if (GameOver) return;
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (UseMoney(Cost))
            {
                AddSpawnCommand(Faction.Red, SolderType.Melee);
                UIManager.Instance.SetMoney(Faction.Red, Money);
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (UseMoney(Cost))
            {
                UIManager.Instance.SetMoney(Faction.Red, Money);
                AddSpawnCommand(Faction.Red, SolderType.Shooting);
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            if (UseMoney(Cost))
            {
                UIManager.Instance.SetMoney(Faction.Red, Money);
                AddSpawnCommand(Faction.Red, SolderType.Mining);
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            if (UseMoney(Cost))
            {
                UIManager.Instance.SetMoney(Faction.Blue, Money);
                AddSpawnCommand(Faction.Blue, SolderType.Melee);
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            if (UseMoney(Cost))
            {
                UIManager.Instance.SetMoney(Faction.Blue, Money);
                AddSpawnCommand(Faction.Blue, SolderType.Shooting);
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            if (UseMoney(Cost))
            {
                UIManager.Instance.SetMoney(Faction.Blue, Money);
                AddSpawnCommand(Faction.Blue, SolderType.Mining);
            }
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
                    Money += (int)value.value;
                    UIManager.Instance.SetMoney(value.faction, Money);
                    break;
            }
        }
    }

    private void OnDestroy()
    {
        Facade.Cleanup();
    }
}