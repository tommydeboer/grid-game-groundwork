using DG.Tweening;
using UnityEngine;

public class Player : Mover
{
    Vector3Int direction = Vector3Int.zero;
    Ladder onLadder;
    public override BlockType Type => BlockType.Player;

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
            direction = Vector3Int.right;
        }
        else if (hor == -1)
        {
            direction = Vector3Int.left;
        }
        else if (ver == -1)
        {
            direction = Vector3Int.back;
        }
        else if (ver == 1)
        {
            direction = Vector3Int.forward;
        }

        var q = Quaternion.LookRotation(direction);
        transform.DORotate(q.eulerAngles, 0.1f);


        TryPlayerMove(direction);
        Game.instance.DoScheduledMoves();
    }


    void TryPlayerMove(Vector3Int dir)
    {
        Vector3Int posToCheck = Tile.gridPos + dir;
        var abovePlayer = Tile.gridPos + Vector3Int.up;

        if (Grid.HasLadderAtPos(posToCheck) &&
            Grid.IsEmpty(abovePlayer))
        {
            var aboveLadder = posToCheck + Vector3Int.up;

            if (Grid.HasLadderAtPos(aboveLadder))
            {
                // climb ladder
                ScheduleMove(Vector3Int.up);
                onLadder = Grid.GetLadderAtPos(aboveLadder);
            }
            else if (Grid.IsEmpty(aboveLadder))
            {
                // climb over edge
                ScheduleMove(Vector3Int.up + dir);
                onLadder = null;
            }
        }
        else if (Grid.HasMoverAtPos(posToCheck))
        {
            if (TryMove(direction))
            {
                ScheduleMove(direction);
                onLadder = null;
            }
        }
        else if (Grid.IsEmpty(posToCheck))
        {
            ScheduleMove(direction);
            onLadder = null;
        }
    }

    protected override bool ShouldFall()
    {
        return !onLadder && base.ShouldFall();
    }
}