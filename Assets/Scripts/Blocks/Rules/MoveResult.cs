using System;
using Vector3 = UnityEngine.Vector3;

namespace GridGame.Blocks.Rules
{
    //TODO use record
    public class MoveResult
    {
        public MoveType Type;
        public Vector3 Vector;
        
        //TODO DidMove based on Type
        public bool DidMove { get; }

        MoveResult(bool didMove, Vector3 vector, MoveType type)
        {
            DidMove = didMove;
            Vector = vector;
            Type = type;
        }

        public static MoveResult Of(bool didMove, Vector3 vector, MoveType type = MoveType.SLIDE)
        {
            return new MoveResult(didMove, vector, type);
        }

        public static MoveResult Success(Vector3 vector, MoveType type = MoveType.SLIDE)
        {
            return new MoveResult(true, vector, type);
        }

        public static MoveResult Failed()
        {
            return new MoveResult(false, Vector3.zero, MoveType.NONE);
        }
    }
}