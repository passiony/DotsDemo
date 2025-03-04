using Unity.Collections;

public struct RcvData
{
    public Faction faction;
    public int type;
    public float value;

    public RcvData(Faction faction,int type,float value)
    {
        this.faction = faction;
        this.type = type;
        this.value = value;
    }
}

public static class Facade
{
    public static NativeQueue<RcvData> SharedQueue { get; } = new(Allocator.Persistent);

    public static void Cleanup()
    {
        if (SharedQueue.IsCreated)
        {
            SharedQueue.Dispose();
        }
    }
}