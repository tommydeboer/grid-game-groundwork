using System;
using UnityEngine;

namespace GridGame
{
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right,
        Forward,
        Back
    }

    static class DirectionExtensions
    {
        public static Vector3Int AsVector(this Direction direction)
        {
            return direction switch
            {
                Direction.Up => Vector3Int.up,
                Direction.Down => Vector3Int.down,
                Direction.Left => Vector3Int.left,
                Direction.Right => Vector3Int.right,
                Direction.Forward => Vector3Int.forward,
                Direction.Back => Vector3Int.back,
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
            };
        }
    }
}