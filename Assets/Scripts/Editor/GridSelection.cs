using System;
using UnityEngine;

namespace Editor
{
    internal class GridSelection
    {
        Vector3Int StartPos { get; }
        public Vector3Int CurrentPos { get; set; }
        public Plane Plane { get; }
        public int Height { get; set; }

        public GridSelection(Vector3Int startPos)
        {
            StartPos = startPos;
            CurrentPos = startPos;
            Plane = new Plane(Vector3.up, startPos);
            Height = 0;
        }

        public void ForEach(Action<Vector3Int> fun)
        {
            int minX = Math.Min(StartPos.x, CurrentPos.x);
            int maxX = Math.Max(StartPos.x, CurrentPos.x);
            int minZ = Math.Min(StartPos.z, CurrentPos.z);
            int maxZ = Math.Max(StartPos.z, CurrentPos.z);
            int minY = Math.Min(StartPos.y, StartPos.y + Height);
            int maxY = Math.Max(StartPos.y, StartPos.y + Height);

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    for (int z = minZ; z <= maxZ; z++)
                    {
                        var pos = new Vector3Int(x, y, z);
                        fun(pos);
                    }
                }
            }
        }
    }
}