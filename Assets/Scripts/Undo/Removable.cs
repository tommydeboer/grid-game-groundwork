using System;
using UnityEngine;

namespace GridGame.Undo
{
    public class Removable : MonoBehaviour, IUndoable, IRemovable
    {
        public bool IsAlive { get; private set; } = true;

        IRemovable[] removables;

        void Awake()
        {
            removables = GetComponents<IRemovable>();
        }

        public void Remove()
        {
            IsAlive = false;
            foreach (IRemovable removable in removables)
            {
                removable.OnRemove();
            }
        }

        void Replace()
        {
            IsAlive = true;
            foreach (IRemovable removable in removables)
            {
                removable.OnReplace();
            }
        }

        class RemovableState : PersistableState
        {
            public bool isAlive;
        }

        public PersistableState GetState()
        {
            return new RemovableState
            {
                isAlive = IsAlive
            };
        }

        public void ApplyState(PersistableState persistableState)
        {
            var state = persistableState.As<RemovableState>();
            if (state.isAlive != IsAlive)
            {
                if (state.isAlive)
                {
                    Replace();
                }
                else
                {
                    Remove();
                }
            }
        }

        public void OnRemove()
        {
            ToggleColliders(false);
            ToggleRenderers(false);
        }

        public void OnReplace()
        {
            ToggleColliders(true);
            ToggleRenderers(true);
        }

        void ToggleColliders(bool enable)
        {
            foreach (var c in GetComponentsInChildren<Collider>())
            {
                c.enabled = enable;
            }
        }

        void ToggleRenderers(bool enable)
        {
            foreach (var r in GetComponentsInChildren<Renderer>())
            {
                r.enabled = enable;
            }
        }
    }
}