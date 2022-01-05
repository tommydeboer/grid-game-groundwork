using UnityEngine;

public class Player : Mover
{
    Vector3 direction = Vector3.zero;
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
            direction = Vector3.right;
        }
        else if (hor == -1)
        {
            direction = Vector3.left;
        }
        else if (ver == -1)
        {
            direction = Vector3.back;
        }
        else if (ver == 1)
        {
            direction = Vector3.forward;
        }

        TryPlayerMove(direction);
        Game.instance.DoScheduledMoves();
    }


    void TryPlayerMove(Vector3 dir)
    {
        Vector3Int posToCheck = Vector3Int.RoundToInt(playerTile.pos + dir);

        if (PositionBuffer.LadderIsAtPos(posToCheck) &&
            PositionBuffer.IsEmpty(Vector3Int.RoundToInt(playerTile.pos) + Vector3Int.up))
        {
            ScheduleMove(Vector3Int.up);
            onLadder = true;
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