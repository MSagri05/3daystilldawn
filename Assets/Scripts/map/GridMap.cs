using UnityEngine;

// Default grid map implementation
public class GridMap : BaseMap
{
    public override void OnMapLoad()
    {
        Debug.Log($"GridMap loaded ({width}x{height})");
    }

    public override void OnMapUnload()
    {
        Debug.Log("GridMap unloaded");
    }
}
