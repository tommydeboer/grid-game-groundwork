using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Blocks;
using UnityEngine;
using DG.Tweening;

public class Game : MonoBehaviour
{
    public delegate void GameEvent();

    public static GameEvent onUndo;
    public static GameEvent onReset;
    public static GameEvent onMoveComplete;

    public static Game instance;

    public static Mover[] movers;
    public static readonly List<Mover> moversToMove = new();

    public float moveTime = 0.18f; // time it takes to move 1 unit
    public float fallTime = 0.1f; // time it takes to fall 1 unit

    public static bool isMoving;
    public int movingCount;
    public bool holdingUndo;
    public static bool isPolyban = true;

    void Awake()
    {
        instance = this;
        Application.targetFrameRate = 60;
    }

    void Start()
    {
        movers = FindObjectsOfType<Mover>();
        State.Init(movers);
        isMoving = false;
    }

    public void EditorRefresh()
    {
        movers = FindObjectsOfType<Mover>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            holdingUndo = true;
            DoUndo();
            DOVirtual.DelayedCall(0.75f, UndoRepeat);
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            DoReset();
        }

        if (Input.GetKeyUp(KeyCode.Z))
        {
            StartCoroutine(StopUndoing());
        }
    }

    void Refresh()
    {
        isMoving = false;
        Debug.Assert(movingCount == 0, "Not all movers have finished");
        moversToMove.Clear();
        movingCount = 0;
    }

    /////////////////////////////////////////////////////////////////// UNDO / RESET

    void DoReset()
    {
        DOTween.KillAll();
        isMoving = false;
        State.DoReset();
        Refresh();
        if (onReset != null)
        {
            onReset();
        }
    }

    void DoUndo()
    {
        if (State.undoIndex > 0)
        {
            DOTween.KillAll();
            if (isMoving)
            {
                CompleteMove();
            }

            isMoving = false;
            State.DoUndo();
            Refresh();
            if (onUndo != null)
            {
                onUndo();
            }
        }
    }

    void UndoRepeat()
    {
        if (Input.GetKey(KeyCode.Z) && holdingUndo)
        {
            DoUndo();
            DOVirtual.DelayedCall(0.075f, UndoRepeat);
        }
    }

    IEnumerator StopUndoing()
    {
        yield return WaitFor.EndOfFrame;
        holdingUndo = false;
    }

    /////////////////////////////////////////////////////////////////// MOVE

    public void DoScheduledMoves()
    {
        if (moversToMove.Count == 0) return;
        isMoving = true;
        foreach (Mover m in moversToMove)
        {
            movingCount++;
            m.transform.DOMove(m.goalPosition, moveTime).OnComplete(MoveEnd).SetEase(Ease.Linear);
        }
    }

    void MoveEnd()
    {
        movingCount--;
        if (movingCount == 0)
        {
            Grid.Refresh();
            FallStart();
        }
    }

    void FallStart()
    {
        isMoving = true;
        movers = movers.OrderBy(mover => mover.transform.position.y).ToArray();

        foreach (Mover m in movers)
        {
            m.FallStart();
        }

        if (movingCount == 0)
        {
            FallEnd();
        }
    }

    public void FallEnd()
    {
        Grid.Refresh();
        if (movingCount == 0)
        {
            Refresh();
            CompleteMove();
        }
    }

    static void CompleteMove()
    {
        State.OnMoveComplete();
        onMoveComplete?.Invoke();
    }
}