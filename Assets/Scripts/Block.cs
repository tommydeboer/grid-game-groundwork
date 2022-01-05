using System.Collections.Generic;
using UnityEngine;

public abstract class Block : MonoBehaviour
{
    public abstract BlockType Type { get; }

    public readonly List<Tile> tiles = new();

    void Awake()
    {
        CreateTiles();
    }

    void CreateTiles()
    {
        tiles.Clear();
        foreach (Transform child in transform)
        {
            if (child.gameObject.CompareTag("Tile"))
            {
                Tile tile = new Tile {t = child};
                tiles.Add(tile);
            }
        }
    }
}