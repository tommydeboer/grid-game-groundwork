using UnityEngine;

public class OutletEnd : Pipe
{
    protected override bool CanMove(Vector3 dir) {
        return false;
    }
}
