using System.Numerics;
using Vector3 = UnityEngine.Vector3;

namespace GridGame.Blocks.Interactions
{
    public class MoveResult
    {
        public Vector3 Vector;
        public bool DidMove { get; }

        MoveResult(bool didMove, Vector3 vector)
        {
            DidMove = didMove;
            Vector = vector;
        }

        public static MoveResult Of(bool didMove, Vector3 vector)
        {
            return new MoveResult(didMove, vector);
        }

        public static MoveResult Success(Vector3 vector)
        {
            return new MoveResult(true, vector);
        }

        public static MoveResult Failed()
        {
            return new MoveResult(false, Vector3.zero);
        }
    }
}