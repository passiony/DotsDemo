using Unity.Entities;

public struct SharedEcsData : IComponentData
{
    public int key;
    public int value;
}