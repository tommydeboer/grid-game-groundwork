using JetBrains.Annotations;
using UnityEngine;

namespace GridGame.Blocks
{
    public class GridElement : MonoBehaviour
    {
        [ReadOnly, UsedImplicitly]
        public int id;

        public bool IsDynamic { get; private set; }
        public Vector3 Position => transform.position;
        public Vector3 Rotation => transform.eulerAngles;
        public Vector3Int Orientation => Vector3Int.RoundToInt(Quaternion.Euler(Rotation) * Vector3.back);
        public Vector3 Below => Position + Vector3.down;

        void Awake()
        {
            IsDynamic = GetComponent<Movable>();
            id = GetInstanceID();
        }

        [CanBeNull]
        public Block GetNeighbour(Vector3Int direction)
        {
            return Utils.GetBlockAtPos(Position + direction);
        }

        [CanBeNull]
        public T GetNeighbouring<T>(Vector3Int direction) where T : GridBehaviour
        {
            return GetNeighbour(direction)?.GetComponent<T>();
        }

        public bool HasNeighbouringOriented<T>(Vector3Int direction, Vector3Int orientation) where T : BlockBehaviour
        {
            // TODO add an out parameter
            var neighbour = GetNeighbour(direction);
            return neighbour && neighbour.GetComponent<T>() && neighbour.Orientation == orientation;
        }

        public bool HasEmptyAt(Vector3Int direction)
        {
            return GetNeighbour(direction) == null;
        }

        public bool Is<T>() where T : MonoBehaviour
        {
            return GetComponent<T>();
        }

        readonly Collider[] intersections = new Collider[3];

        public bool Intersects<T>() where T : GridElement
        {
            int hits = Physics.OverlapBoxNonAlloc(
                Position,
                Vector3.one * .49f,
                intersections,
                Quaternion.identity,
                (int)Layers.GridPhysics
            );

            for (int i = 0; i < hits; i++)
            {
                if (intersections[i].gameObject.GetComponentInParent<GridElement>() == this) continue;
                if (intersections[i].gameObject.GetComponentInParent<T>())
                {
                    return true;
                }
            }

            return false;
        }

        // TODO remove when GetNeighbour returns edge before block
        public bool IsOriented<T>(Vector3Int orientation) where T : BlockBehaviour
        {
            return GetComponent<T>() && Orientation == orientation;
        }
    }
}