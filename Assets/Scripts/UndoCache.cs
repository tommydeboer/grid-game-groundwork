using System;
using System.Collections.Generic;
using GridGame.Blocks;
using GridGame.Player;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GridGame
{
    public class UndoCache
    {
        struct MoverState
        {
            public Movable mover;
            public List<Vector3> positions;
            public List<Vector3> rotations;
        }

        struct HeroState
        {
            public Hero hero;
            public List<Climbable> onClimbable;
        }

        readonly Grid grid;

        List<MoverState> moversToTrack = new();
        HeroState heroToTrack;
        public int undoIndex;

        public UndoCache(Grid grid)
        {
            this.grid = grid;
        }

        void AddMover(Movable movable)
        {
            var tf = movable.transform;
            MoverState newMover = new MoverState
            {
                mover = movable,
                positions = new List<Vector3>(),
                rotations = new List<Vector3>()
            };
            newMover.positions.Add(tf.position);
            newMover.rotations.Add(tf.eulerAngles);
            moversToTrack.Add(newMover);

            // TODO very inefficient
            if (movable.GetComponent<Hero>() != null)
            {
                heroToTrack.hero = movable.GetComponent<Hero>();
                heroToTrack.onClimbable = new List<Climbable>();
            }
        }

        public void Reset()
        {
            moversToTrack = new List<MoverState>();
            heroToTrack = new HeroState();
            undoIndex = 0;

            foreach (Movable mover in grid.GetMovers())
            {
                AddMover(mover);
            }

            AddToUndoStack();
        }

        void AddToUndoStack()
        {
            foreach (MoverState m in moversToTrack)
            {
                m.positions.Add(m.mover.transform.position);
                m.rotations.Add(m.mover.transform.eulerAngles);
            }

            heroToTrack.onClimbable.Add(heroToTrack.hero.OnClimbable);
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

            heroToTrack.onClimbable.RemoveAt(heroToTrack.onClimbable.Count - 1);
            heroToTrack.hero.OnClimbable = heroToTrack.onClimbable[^1];
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
                heroToTrack.hero.IsAlive = true;
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
                tf.position = m.positions[0];
                tf.eulerAngles = m.rotations[0];
            }

            heroToTrack.hero.OnClimbable = heroToTrack.onClimbable[0];
            heroToTrack.hero.IsAlive = true;

            OnMoveComplete();
            grid.Refresh();
            foreach (var item in grid.GetMovers())
            {
                item.isFalling = false;
            }
        }
    }
}