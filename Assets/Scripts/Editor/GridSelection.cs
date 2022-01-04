using System;
using UnityEngine;

namespace Editor
{
    internal class GridSelection
    {
        // TODO implement IEnumerable?

        readonly int verticalOffset;
        readonly Collider[] overlapBuffer = new Collider[512];
        Vector3Int StartPos { get; }
        Vector3Int endPos;

        public Vector3Int EndPos
        {
            get => endPos;
            set => endPos = value + (Vector3Int.up * verticalOffset);
        }

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
                var b2 = new Bounds(EndPos + (Vector3Int.up * Height), Vector3.one);
                b1.Encapsulate(b2);
                return b1;
            }
        }

        public Vector3[] Intersections
        {
            get
            {
                var bounds = Bounds;
                bounds.Expand(-Vector3.one * 0.9f);

                int size = Physics.OverlapBoxNonAlloc(bounds.center, bounds.extents, overlapBuffer,
                    Quaternion.identity);

                var points = new Vector3[size];
                for (int i = 0; i < size; i++)
                {
                    points[i] = overlapBuffer[i].transform.position;
                }

                return points;
            }
        }

        public GridSelection(Vector3Int startPos)
        {
            StartPos = startPos;
            EndPos = startPos;
            Plane = new Plane(Vector3.up, startPos);
            Height = 0;
            verticalOffset = 0;
        }

        public GridSelection(Vector3Int startPos, int verticalOffset)
        {
            StartPos = startPos;
            EndPos = startPos;
            Plane = new Plane(Vector3.up, startPos - (Vector3Int.up * verticalOffset));
            Height = 0;
            this.verticalOffset = verticalOffset;
        }

        public void ForEach(Action<Vector3Int> action)
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
                        action(pos);
                    }
                }
            }
        }
    }
}