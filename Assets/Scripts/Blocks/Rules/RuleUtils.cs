using UnityEngine;

namespace GridGame.Blocks.Rules
{
    public static class RuleUtils
    {
        public static bool MovedOutOfBounds(GridElement element, MoveResult result)
        {
            if (result.DidMove)
            {
                return !(Physics.Raycast(
                    element.Position + result.Vector + Vector3.down * .45f,
                    Vector3.down,
                    out RaycastHit _,
                    Mathf.Infinity,
                    (int)Layers.GridPhysics));
            }

            return false;
        }
    }
}