using UnityEngine;

namespace GridGame.Editor
{
    public static class BoundsExtensions
    {
        public static float GetVolume(this Bounds bounds)
        {
            var size = bounds.size;
            return size.x * size.y * size.z;
        }
    }
}