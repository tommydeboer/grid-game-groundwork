using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using JetBrains.Annotations;
using UnityEngine;

namespace GridGame.Blocks
{
    public class Block : MonoBehaviour
    {
        [SerializeField]
        BlockMaterial material = BlockMaterial.Default;

        public BlockMaterial Material => material;
        public Tile Tile { get; private set; }
        public Vector3Int Orientation => Vector3Int.RoundToInt(Quaternion.Euler(Tile.rot) * Vector3.back);

        void Awake()
        {
            CreateTile();
        }

        public Vector3Int Below => Tile.gridPos + Vector3Int.down;

        [CanBeNull]
        public Block GetNeighbour(Vector3Int direction)
        {
            // TODO use layer mask for grid objects

            RaycastHit hit;
            if (Physics.Raycast(transform.position, direction, out hit, 1f))
            {
                return hit.collider.gameObject.GetComponentInParent<Block>();
            }

            return null;
        }

        [CanBeNull]
        public T GetNeighbouring<T>(Vector3Int direction) where T : BlockBehaviour
        {
            return GetNeighbour(direction)?.GetComponent<T>();
        }

        public bool HasNeighbouring<T>(Vector3Int direction) where T : BlockBehaviour
        {
            // TODO add an out parameter
            var neighbour = GetNeighbour(direction);
            return neighbour && neighbour.GetComponent<T>();
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

        public bool Is<T>() where T : BlockBehaviour
        {
            return GetComponent<T>();
        }

        readonly Collider[] intersections = new Collider[3];

        public bool Intersects<T>() where T : BlockBehaviour
        {
            int hits = Physics.OverlapBoxNonAlloc(Tile.gridPos, Vector3.one * .49f, intersections);
            for (int i = 0; i < hits; i++)
            {
                if (intersections[i].gameObject.GetComponentInParent<Block>() == this) continue;
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

        void CreateTile()
        {
            bool found = false;
            foreach (Transform child in transform)
            {
                if (child.gameObject.CompareTag(Tags.TILE))
                {
                    if (found)
                    {
                        Debug.LogWarning("Block contains more than one tile", this);
                    }

                    Tile = new Tile { t = child };
                    found = true;
                }
            }

            if (!found)
            {
                Debug.LogWarning("Block contains no tile", this);
            }
        }
    }
}