namespace GridGame
{
    [System.Flags]
    public enum Layers
    {
        Default = 1 << 0,
        TransparentFX = 1 << 1,
        IgnoreRaycast = 1 << 2,
        Water = 1 << 4,
        UI = 1 << 5,
        PostProcessing = 1 << 8,
        GridPhysics = 1 << 9
    }
}