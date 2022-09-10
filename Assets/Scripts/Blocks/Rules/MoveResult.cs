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
        public MoveResult MovedOther { get; }
        public bool DidMoveOther => MovedOther != null;

        MoveResult(bool didMove, Vector3 vector, MoveType type, MoveResult movedOther)
        {
            DidMove = didMove;
            Vector = vector;
            Type = type;
            MovedOther = movedOther;
        }

        public static MoveResult Of(MoveResult other, MoveType type = MoveType.SLIDE)
        {
            return new MoveResult(other.DidMove, other.Vector, type, other);
        }

        public static MoveResult Success(Vector3 vector, MoveType type = MoveType.SLIDE)
        {
            return new MoveResult(true, vector, type, null);
        }

        public static MoveResult Failed()
        {
            return new MoveResult(false, Vector3.zero, MoveType.NONE, null);
        }

        public static MoveResult FailedBy(MoveResult moveOther)
        {
            return new MoveResult(false, Vector3.zero, MoveType.NONE, moveOther);
        }
    }
}