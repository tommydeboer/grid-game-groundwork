using UnityEngine;

namespace GridGame.Blocks.Interactions
{
    public class MoveResult
    {
        public bool DidMove { get; }
        public Vector3 Offset { get; } = Vector3.zero;

        MoveResult(bool didMove)
        {
            DidMove = didMove;
        }

        MoveResult(bool didMove, Vector3 offset)
        {
            DidMove = didMove;
            Offset = offset;
        }

        public static MoveResult Of(bool didMove)
        {
            return new MoveResult(didMove);
        }

        public static MoveResult Success()
        {
            return new MoveResult(true);
        }

        public static MoveResult Success(Vector3 offset)
        {
            return new MoveResult(true, offset);
        }

        public static MoveResult Failed()
        {
            return new MoveResult(false);
        }
    }
}