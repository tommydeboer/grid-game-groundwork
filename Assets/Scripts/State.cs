using System.Collections.Generic;
using Blocks;
using UnityEngine;

public static class State
{
    struct MoverToTrack
    {
        public Mover mover;
        public Vector3 initialPos;
        public Vector3 initialRot;
        public List<Vector3Int> positions;
        public List<Vector3Int> rotations;
    }

    static readonly List<MoverToTrack> moversToTrack = new();
    public static int undoIndex;

    static void AddMover(Mover mover)
    {
        var transform = mover.transform;
        MoverToTrack newMover = new MoverToTrack
        {
            mover = mover,
            initialPos = transform.position,
            initialRot = transform.eulerAngles,
            positions = new List<Vector3Int>(),
            rotations = new List<Vector3Int>()
        };
        moversToTrack.Add(newMover);
    }

    public static void Init(IEnumerable<Mover> movers)
    {
        foreach (Mover mover in movers)
        {
            AddMover(mover);
        }

        AddToUndoStack();
    }

    static void AddToUndoStack()
    {
        foreach (MoverToTrack m in moversToTrack)
        {
            m.positions.Add(Vector3Int.RoundToInt(m.mover.transform.position));
            m.rotations.Add(Vector3Int.RoundToInt(m.mover.transform.eulerAngles));
        }
    }

    static void RemoveFromUndoStack()
    {
        foreach (MoverToTrack m in moversToTrack)
        {
            m.positions.RemoveAt(m.positions.Count - 1);
            m.rotations.RemoveAt(m.rotations.Count - 1);
            var transform = m.mover.transform;
            transform.position = m.positions[^1];
            transform.eulerAngles = m.rotations[^1];
        }
    }

    public static void OnMoveComplete()
    {
        undoIndex++;
        AddToUndoStack();
    }

    public static void DoUndo()
    {
        if (undoIndex > 0)
        {
            undoIndex--;
            RemoveFromUndoStack();
            Grid.Refresh();
            foreach (var item in Game.movers)
            {
                item.isFalling = false;
            }
        }
    }

    public static void DoReset()
    {
        foreach (MoverToTrack m in moversToTrack)
        {
            var transform = m.mover.transform;
            transform.position = m.initialPos;
            transform.eulerAngles = m.initialRot;
        }

        OnMoveComplete();
        Grid.Refresh();
        foreach (var item in Game.movers)
        {
            item.isFalling = false;
        }
    }
}