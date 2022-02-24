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