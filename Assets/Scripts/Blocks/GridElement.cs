using JetBrains.Annotations;
using UnityEngine;

namespace GridGame.Blocks
{
    public class GridElement : MonoBehaviour
    {
        [ReadOnly, UsedImplicitly]
        public int id;

        public bool IsDynamic => Movable != null;
        public Movable Movable { get; private set; }
        public Vector3 Position => transform.position;
        public Vector3 Rotation => transform.eulerAngles;
        public Vector3Int Orientation => Vector3Int.RoundToInt(Quaternion.Euler(Rotation) * Vector3.back);
        public Vector3 Below => Position + Vector3.down;

        [CanBeNull]
        public Block BlockBelow => GetNeighbour(Direction.Down);

        [CanBeNull]
        public Block BlockAbove => GetNeighbour(Direction.Up);

        void Awake()
        {
            Movable = GetComponent<Movable>();
            id = GetInstanceID();
        }

        [CanBeNull]
        public Block GetNeighbour(Direction direction)
        {
            return Utils.GetBlockAtPos(Position + direction.AsVector());
        }

        [CanBeNull]
        public T GetNeighbouring<T>(Direction direction) where T : GridBehaviour
        {
            return GetNeighbour(direction)?.GetComponent<T>();
        }

        public bool HasNeighbouringOriented<T>(Direction direction, Vector3Int orientation) where T : BlockBehaviour
        {
            // TODO add an out parameter
            var neighbour = GetNeighbour(direction);
            return neighbour && neighbour.GetComponent<T>() && neighbour.Orientation == orientation;
        }

        public bool HasEmptyAt(Direction direction)
        {
            return GetNeighbour(direction) == null;
        }

        public bool Is<T>() where T : MonoBehaviour
        {
            return GetComponent<T>();
        }

        readonly Collider[] intersections = new Collider[3];

        protected bool Intersects<T>() where T : GridElement
        {
            return GetIntersects<T>();
        }

        public T GetIntersects<T>() where T : GridElement
        {
            int hits = Physics.OverlapBoxNonAlloc(
                Position,
                Vector3.one * .1f,
                intersections,
                Quaternion.identity,
                (int)Layers.GridPhysics
            );

            for (int i = 0; i < hits; i++)
            {
                var target = intersections[i].gameObject.GetComponentInParent<T>();
                if (target)
                {
                    if (target == this) continue;
                    return target;
                }
            }

            return null;
        }

        // TODO remove when GetNeighbour returns edge before block
        public bool IsOriented<T>(Vector3Int orientation) where T : BlockBehaviour
        {
            return GetComponent<T>() && Orientation == orientation;
        }
    }
}