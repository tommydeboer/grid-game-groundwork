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

        public Vector3Int MinCorner
        {
            get
            {
                int minX = Math.Min(StartPos.x, CurrentPos.x);
                int minZ = Math.Min(StartPos.z, CurrentPos.z);
                int minY = Math.Min(StartPos.y, StartPos.y + Height);

                return new Vector3Int(minX, minY, minZ);
            }
        }

        public Vector3Int MaxCorner
        {
            get
            {
                int maxX = Math.Max(StartPos.x, CurrentPos.x);
                int maxZ = Math.Max(StartPos.z, CurrentPos.z);
                int maxY = Math.Max(StartPos.y, StartPos.y + Height);

                return new Vector3Int(maxX, maxY, maxZ);
            }
        }

        public GridSelection(Vector3Int startPos)
        {
            StartPos = startPos;
            CurrentPos = startPos;
            Plane = new Plane(Vector3.up, startPos);
            Height = 0;
        }

        public void ForEach(Action<Vector3Int> fun)
        {
            var minCorner = MinCorner;
            var maxCorner = MaxCorner;

            for (int y = minCorner.y; y <= maxCorner.y; y++)
            {
                for (int x = minCorner.x; x <= maxCorner.x; x++)
                {
                    for (int z = minCorner.z; z <= maxCorner.z; z++)
                    {
                        var pos = new Vector3Int(x, y, z);
                        fun(pos);
                    }
                }
            }
        }
    }
}