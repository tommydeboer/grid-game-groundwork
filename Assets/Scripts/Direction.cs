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

    internal static class DirectionExtensions
    {
        public static Vector3 AsVector(this Direction direction)
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

        public static Quaternion RotateTo(this Direction direction, Direction target)
        {
            if (direction == Direction.Up)
            {
                return target switch
                {
                    Direction.Up => Quaternion.Euler(0, 0, 0),
                    Direction.Down => Quaternion.Euler(180, 0, 0),
                    Direction.Left => Quaternion.Euler(0, 0, 90),
                    Direction.Right => Quaternion.Euler(0, 0, -90),
                    Direction.Forward => Quaternion.Euler(90, 0, 0),
                    Direction.Back => Quaternion.Euler(-90, 0, 0),
                    _ => throw new ArgumentOutOfRangeException(nameof(target), target, null)
                };
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public static Direction Opposite(this Direction direction)
        {
            return direction switch
            {
                Direction.Up => Direction.Down,
                Direction.Down => Direction.Up,
                Direction.Left => Direction.Right,
                Direction.Right => Direction.Left,
                Direction.Forward => Direction.Back,
                Direction.Back => Direction.Forward,
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
            };
        }
    }
}