using System;
using UnityEngine;

namespace Editor
{
    internal class GridSelection
    {
        // TODO implement IEnumerable?

        Vector3Int StartPos { get; }
        public Vector3Int CurrentPos { get; set; }
        public Plane Plane { get; }
        public int Height { get; set; }

        public int Count
        {
            get
            {
                var size = Bounds.size;
                return (int) size.x * (int) size.y * (int) size.z;
            }
        }

        public Bounds Bounds
        {
            get
            {
                var b1 = new Bounds(StartPos, Vector3.one);
                var b2 = new Bounds(CurrentPos + (Vector3Int.up * Height), Vector3.one);
                b1.Encapsulate(b2);
                return b1;
            }
        }

        public bool Intersects
        {
            get
            {
                // TODO improve performance by doing one big overlap test 

                bool intersects = false;
                ForEach(pos =>
                {
                    if (!Utils.TileIsEmpty(pos))
                    {
                        intersects = true;
                    }
                });
                return intersects;
            }
        }

        public GridSelection(Vector3Int startPos)
        {
            StartPos = startPos;
            CurrentPos = startPos;
            Plane = new Plane(Vector3.up, startPos);
            Height = 0;
        }

        public GridSelection(Vector3Int startPos, int verticalOffset)
        {
            StartPos = startPos;
            CurrentPos = startPos;
            Plane = new Plane(Vector3.up, startPos - (Vector3Int.up * verticalOffset));
            Height = 0;
        }

        public void ForEach(Action<Vector3Int> fun)
        {
            var bounds = Bounds;
            var minCorner = Vector3Int.CeilToInt(bounds.min);
            var maxCorner = Vector3Int.FloorToInt(bounds.max);

            for (int y = minCorner.y;
                y <= maxCorner.y;
                y++)
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