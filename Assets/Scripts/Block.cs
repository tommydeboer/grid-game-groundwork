using System.Collections.Generic;
using UnityEngine;

public abstract class Block : MonoBehaviour
{
    public abstract BlockType Type { get; }

    public Tile Tile { get; private set; }

    void Awake()
    {
        CreateTile();
    }

    void CreateTile()
    {
        bool found = false;
        foreach (Transform child in transform)
        {
            if (child.gameObject.CompareTag("Tile"))
            {
                if (found)
                {
                    Debug.LogWarning("Block contains more than one tile", this);
                }

                Tile = new Tile {t = child};
                found = true;
            }
        }

        if (!found)
        {
            Debug.LogWarning("Block contains no tile", this);
        }
    }
}