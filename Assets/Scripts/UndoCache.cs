using System;
using System.Collections.Generic;
using GridGame.Blocks;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GridGame
{
    public class UndoCache
    {
        struct MoverState
        {
            public Mover mover;
            public Vector3 initialPos;
            public Vector3 initialRot;
            public List<Vector3Int> positions;
            public List<Vector3Int> rotations;
        }

        readonly Grid grid;

        List<MoverState> moversToTrack = new();
        public int undoIndex;

        public UndoCache(Grid grid)
        {
            this.grid = grid;
        }

        void AddMover(Mover mover)
        {
            var tf = mover.transform;
            MoverState newMover = new MoverState
            {
                mover = mover,
                initialPos = tf.position,
                initialRot = tf.eulerAngles,
                positions = new List<Vector3Int>(),
                rotations = new List<Vector3Int>()
            };
            moversToTrack.Add(newMover);
        }

        public void Reset()
        {
            moversToTrack = new List<MoverState>();
            undoIndex = 0;

            foreach (Mover mover in grid.GetMovers())
            {
                AddMover(mover);
            }

            AddToUndoStack();
        }

        void AddToUndoStack()
        {
            foreach (MoverState m in moversToTrack)
            {
                m.positions.Add(Vector3Int.RoundToInt(m.mover.transform.position));
                m.rotations.Add(Vector3Int.RoundToInt(m.mover.transform.eulerAngles));
            }
        }

        void RemoveFromUndoStack()
        {
            foreach (MoverState m in moversToTrack)
            {
                m.positions.RemoveAt(m.positions.Count - 1);
                m.rotations.RemoveAt(m.rotations.Count - 1);
                var tf = m.mover.transform;
                tf.position = m.positions[^1];
                tf.eulerAngles = m.rotations[^1];
            }
        }

        public void OnMoveComplete()
        {
            undoIndex++;
            AddToUndoStack();
        }

        public void DoUndo()
        {
            if (undoIndex > 0)
            {
                undoIndex--;
                RemoveFromUndoStack();
                grid.Refresh();
                foreach (var item in grid.GetMovers())
                {
                    item.isFalling = false;
                }
            }
        }

        public void DoReset()
        {
            foreach (MoverState m in moversToTrack)
            {
                var tf = m.mover.transform;
                tf.position = m.initialPos;
                tf.eulerAngles = m.initialRot;
            }

            OnMoveComplete();
            grid.Refresh();
            foreach (var item in grid.GetMovers())
            {
                item.isFalling = false;
            }
        }
    }
}