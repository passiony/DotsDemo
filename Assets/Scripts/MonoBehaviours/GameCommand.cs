using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

// 通用命令组件
public struct GameCommand : IBufferElementData {
    public CommandType Type;
    public Faction Faction;
    
    public enum CommandType {
        SpawnUnit,
        DestroyUnit,
        ChangeTeam
    }
}

// 示例命令参数结构（需内存对齐）
// public struct SpawnCommandParams {
//     public Faction Faction;
//     public float3 Position;
//     public int Count;
// }
