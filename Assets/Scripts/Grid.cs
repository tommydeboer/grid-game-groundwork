using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Grid
{
    static Dictionary<Vector3Int, Wall> walls;
    static Dictionary<Vector3Int, Mover> movers;

    static Dictionary<Vector3Int, Wall> Walls
    {
        get
        {
            if (walls == null) Reset();
            return walls;
        }
        set => walls = value;
    }

    static Dictionary<Vector3Int, Mover> Movers
    {
        get
        {
            if (movers == null) Reset();
            return movers;
        }
        set => movers = value;
    }

    static void Reset()
    {
        Walls = new Dictionary<Vector3Int, Wall>();
        Movers = new Dictionary<Vector3Int, Mover>();
        var levelTransform = GameObject.FindWithTag("Level").transform;

        foreach (Transform item in levelTransform)
        {
            var block = item.GetComponentInParent<Block>();
            switch (block)
            {
                case Wall wall:
                    Walls[wall.Tile.gridPos] = wall;
                    break;
                case Mover mover:
                    Movers[mover.Tile.gridPos] = mover;
                    break;
            }
        }
    }

    public static void Refresh()
    {
        var allMovers = Movers.Values;
        Movers = new Dictionary<Vector3Int, Mover>();

        foreach (Mover mover in allMovers)
        {
            Movers[mover.Tile.gridPos] = mover;
        }
    }

    public static T Get<T>(Vector3Int pos) where T : Block
    {
        if (typeof(Wall).IsAssignableFrom(typeof(T)))
        {
            if (Walls.ContainsKey(pos) && Walls[pos] is T t)
            {
                return t;
            }
        }
        else if (typeof(Mover).IsAssignableFrom(typeof(T)))
        {
            if (Movers.ContainsKey(pos) && Movers[pos] is T t)
            {
                return t;
            }
        }

        return null;
    }

    // TODO return block via out param?
    public static bool Has<T>(Vector3Int pos) where T : Block
    {
        return Get<T>(pos) != null;
    }

    public static bool HasOriented<T>(Vector3Int pos, Vector3Int orientation) where T : Block
    {
        var block = Get<T>(pos);
        return block != null && block.Orientation == orientation;
    }

    public static bool IsEmpty(Vector3Int pos)
    {
        return !Walls.ContainsKey(pos);
    }
}