using UnityEngine;

namespace Blocks
{
    public class Tile
    {
        public Transform t;

        public Vector3 pos => t.position;

        public Vector3Int gridPos => Vector3Int.RoundToInt(t.position);

        public Vector3 rot => t.eulerAngles;
    }
}