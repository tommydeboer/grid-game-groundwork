using DG.Tweening;
using UnityEngine;

public class Player : Mover
{
    Vector3Int direction = Vector3Int.zero;
    Ladder onLadder;
    const float LadderOffset = 0.35f;
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

        TryPlayerMove(direction);
        Game.instance.DoScheduledMoves();
    }

    void LookAt(Vector3 dir)
    {
        var q = Quaternion.LookRotation(dir);
        transform.DORotate(q.eulerAngles, 0.1f);
    }


    void TryPlayerMove(Vector3Int dir)
    {
        var playerPos = Tile.gridPos;
        Vector3Int targetPos = playerPos + dir;
        var belowPlayer = playerPos + Vector3Int.down;


        if (onLadder)
        {
            TryClimb(dir, targetPos, playerPos, belowPlayer);
        }
        else if (Grid.HasOriented<Ladder>(belowPlayer, dir) && Grid.IsEmpty(targetPos) &&
                 Grid.IsEmpty(targetPos + Vector3Int.down))
        {
            // mount ladder from above
            ScheduleMove(Vector3Int.down + (Vector3) dir * (1 - LadderOffset));
            onLadder = Grid.Get<Ladder>(belowPlayer);
            LookAt(-dir);
        }
        else if (Grid.HasOriented<Ladder>(targetPos, -dir))
        {
            // mount ladder
            ScheduleMove((Vector3) dir * LadderOffset);
            onLadder = Grid.Get<Ladder>(targetPos);
            LookAt(dir);
        }
        else if (Grid.Has<Mover>(targetPos))
        {
            if (TryMove(dir))
            {
                ScheduleMove(dir);
                onLadder = null;
            }
        }
        else if (Grid.IsEmpty(targetPos))
        {
            ScheduleMove(dir);
            onLadder = null;
        }

        if (!onLadder)
        {
            LookAt(dir);
        }
    }

    void TryClimb(Vector3Int dir, Vector3Int targetPos, Vector3Int playerPos, Vector3Int belowPlayer)
    {
        var abovePlayer = playerPos + Vector3Int.up;
        var ladderPos = onLadder.Tile.gridPos;

        if (targetPos == ladderPos)
        {
            // attempting to climb up ladder that we are touching

            if (Grid.Has<Block>(abovePlayer)) return;

            var aboveLadder = targetPos + Vector3Int.up;
            if (Grid.HasOriented<Ladder>(aboveLadder, -dir))
            {
                // climb up to next ladder
                ScheduleMove(Vector3Int.up);
                onLadder = Grid.Get<Ladder>(aboveLadder);
                LookAt(dir);
            }
            else if (Grid.IsEmpty(aboveLadder))
            {
                // climb up over edge
                ScheduleMove(Vector3Int.up + ((Vector3) dir * (1 - LadderOffset)));
                onLadder = null;
            }
        }
        else if (Grid.HasOriented<Ladder>(ladderPos + dir, onLadder.Orientation))
        {
            // try to climb to neighbouring ladder, and push movers if it's possible
            if (TryMove(direction))
            {
                ScheduleMove(direction);
                onLadder = Grid.Get<Ladder>(ladderPos + dir);
            }
        }
        else if (onLadder == Grid.Get<Ladder>(playerPos - dir))
        {
            // attempting to climb down the ladder that we are touching
            var belowLadder = ladderPos + Vector3Int.down;

            if (Grid.HasOriented<Ladder>(belowLadder, onLadder.Orientation) && Grid.IsEmpty(belowPlayer))
            {
                // climbing down
                ScheduleMove(Vector3Int.down);
                onLadder = Grid.Get<Ladder>(belowLadder);
            }
            else if (Grid.IsEmpty(belowLadder) && Grid.IsEmpty(belowPlayer))
            {
                // falling down
                ScheduleMove(Vector3Int.down + ((Vector3) dir * LadderOffset));
                onLadder = null;
            }
            else
            {
                // stepping off
                ScheduleMove((Vector3) dir * LadderOffset);
                onLadder = null;
            }
        }
        else
        {
            // trying to step off from the ladder sideways
            Vector3 directionToLadder = ((Vector3) playerPos - ladderPos).normalized;
            if (TryMove(dir))
            {
                ScheduleMove(dir + ((Vector3) directionToLadder * LadderOffset));
                onLadder = null;
            }
        }
    }


    protected override bool ShouldFall()
    {
        return !onLadder && base.ShouldFall();
    }
}