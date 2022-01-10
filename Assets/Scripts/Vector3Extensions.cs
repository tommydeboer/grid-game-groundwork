using System.Collections.Generic;
using UnityEngine;

public static class Vector3Extensions
{
    static readonly Dictionary<Vector3Int, string> DirectionStrings = new()
    {
        {Vector3Int.back, "BACK"},
        {Vector3Int.left, "LEFT"},
        {Vector3Int.forward, "FORWARD"},
        {Vector3Int.right, "RIGHT"},
        {Vector3Int.up, "UP"},
        {Vector3Int.down, "DOWN"},
    };

    public static string ToDirectionString(this Vector3Int dir)
    {
        return DirectionStrings.ContainsKey(dir) ? DirectionStrings[dir] : dir.ToString();
    }
}