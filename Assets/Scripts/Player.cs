using UnityEngine;

public class Player : Mover
{
    Vector3Int direction = Vector3Int.zero;
    Tile playerTile;
    bool onLadder;
    public override BlockType Type => BlockType.Player;

    void Start()
    {
        playerTile = tiles[0];
    }

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


    void TryPlayerMove(Vector3Int dir)
    {
        var playerPos = Vector3Int.RoundToInt(playerTile.pos);
        Vector3Int posToCheck = playerPos + dir;
        var abovePlayer = playerPos + Vector3Int.up;

        if (PositionBuffer.LadderIsAtPos(posToCheck) &&
            PositionBuffer.IsEmpty(abovePlayer))
        {
            var aboveLadder = posToCheck + Vector3Int.up;

            if (PositionBuffer.LadderIsAtPos(aboveLadder))
            {
                // climb ladder
                ScheduleMove(Vector3Int.up);
                onLadder = true;
            }
            else if (PositionBuffer.IsEmpty(aboveLadder))
            {
                // climb over edge
                ScheduleMove(Vector3Int.up + dir);
                onLadder = false;
            }
        }
        else if (PositionBuffer.MoverIsAtPos(posToCheck))
        {
            if (TryMove(direction))
            {
                ScheduleMove(direction);
                onLadder = false;
            }
        }
        else if (PositionBuffer.IsEmpty(posToCheck))
        {
            ScheduleMove(direction);
            onLadder = false;
        }
    }

    protected override bool ShouldFall()
    {
        return !onLadder && base.ShouldFall();
    }
}