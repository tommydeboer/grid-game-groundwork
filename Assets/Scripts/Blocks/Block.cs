using UnityEngine;

namespace GridGame.Blocks
{
    public abstract class Block : MonoBehaviour
    {
        public abstract BlockType Type { get; }

        public Tile Tile { get; private set; }
        public Vector3Int Orientation => Vector3Int.RoundToInt(Quaternion.Euler(Tile.rot) * Vector3.back);

        protected Grid grid;

        void Awake()
        {
            CreateTile();
        }

        protected virtual void Start()
        {
            grid = CoreComponents.Grid;
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