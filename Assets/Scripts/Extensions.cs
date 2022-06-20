using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GridGame
{
    public static class Vector3Extensions
    {
        public static bool IsLevel(this Scene scene)
        {
            return scene.name.StartsWith("Level_");
        }

        public static Direction ToDirection(this Vector3Int direction)
        {
            if (direction == Vector3.up) return Direction.Up;
            if (direction == Vector3.down) return Direction.Down;
            if (direction == Vector3.left) return Direction.Left;
            if (direction == Vector3.right) return Direction.Right;
            if (direction == Vector3.forward) return Direction.Forward;
            if (direction == Vector3.back) return Direction.Back;
            throw new ArgumentException("Not a valid direction");
        }
    }
}