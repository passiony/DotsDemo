using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public enum CommandType {
    SpawnUnit,
    DestroyUnit,
    ChangeTeam
}
public enum SolderType {
    Melee,
    Shooting,
    Mining
}

// 通用命令组件
public struct GameCommand : IBufferElementData {
    public CommandType CMD;
    public SolderType Solder;
    public Faction Faction;
    public int Number;
}
