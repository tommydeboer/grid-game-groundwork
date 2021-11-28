using UnityEngine;

public class Tile
{
    public Transform t;

    public Vector3 pos => t.position;

    public Vector3 rot => t.eulerAngles;
}