using UnityEngine;

public class OutletStart : Pipe {
    protected override bool CanMove(Vector3 dir) {
        return false;
    }
}
