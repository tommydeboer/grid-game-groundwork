using UnityEngine;

public class Player : Mover
{
    Vector3 direction = Vector3.zero;

    void Update()
    {
        if (CanInput())
        {
            CheckInput();
        }
    }

    static bool CanInput()
    {
        return !Game.isMoving && !Game.instance.holdingUndo;
    }

    void CheckInput()
    {
        float hor = Input.GetAxisRaw("Horizontal");
        float ver = Input.GetAxisRaw("Vertical");

        if (hor == 0 && ver == 0)
        {
            return;
        }

        if (hor != 0 && ver != 0)
        {
            if (direction == Vector3.right || direction == Vector3.left)
            {
                hor = 0;
            }
            else
            {
                ver = 0;
            }
        }

        if (hor == 1)
        {
            direction = Vector3.right;
        }
        else if (hor == -1)
        {
            direction = Vector3.left;
        }
        else if (ver == -1)
        {
            direction = Vector3.down;
        }
        else if (ver == 1)
        {
            direction = Vector3.up;
        }

        if (CanMove(direction))
        {
            MoveIt(direction);
            Game.instance.MoveStart();
        }
        else
        {
            Game.moversToMove.Clear();
        }
    }
}