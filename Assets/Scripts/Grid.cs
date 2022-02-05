using System.Collections.Generic;
using System.Linq;
using GridGame.Blocks;
using GridGame.Player;
using GridGame.SO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace GridGame
{
    public class Grid : MonoBehaviour
    {
        [SerializeField]
        LoadEventChannelSO loadEventChannel;

        public UnityAction OnGridReset;

        Dictionary<Vector3Int, List<Block>> blocks;

        Dictionary<Vector3Int, List<Block>> Blocks
        {
            get
            {
                if (blocks == null) Reset();
                return blocks;
            }
            set => blocks = value;
        }

        void Awake()
        {
            loadEventChannel.OnLevelLoaded += Reset;
        }

        void Reset()
        {
            Reset(SceneManager.GetActiveScene());
        }

        void Reset(Scene scene)
        {
            Transform levelRoot = scene.GetRootGameObjects().First(go => go.CompareTag(Tags.LEVEL)).transform;

            Blocks = new Dictionary<Vector3Int, List<Block>>();

            foreach (Transform item in levelRoot)
            {
                var block = item.GetComponentInParent<Block>();
                AddBlock(block);
            }

            OnGridReset?.Invoke();
        }

        public void Refresh()
        {
            var allBlocks = Blocks.Values;
            Blocks = new Dictionary<Vector3Int, List<Block>>();

            allBlocks.SelectMany(x => x).ToList().ForEach(AddBlock);
        }

        void AddBlock(Block block)
        {
            var pos = block.Tile.gridPos;
            if (!Blocks.ContainsKey(pos))
            {
                Blocks[block.Tile.gridPos] = new List<Block>();
            }

            Blocks[pos].Add(block);
        }

        public T Get<T>(Vector3Int pos) where T : BlockBehaviour
        {
            if (blocks.ContainsKey(pos))
            {
                return blocks[pos]
                    .Select(block => block.GetComponent<T>())
                    .FirstOrDefault(t => t != null);
            }

            return null;
        }

        public List<T> GetAll<T>(Vector3Int pos) where T : BlockBehaviour
        {
            List<T> blockBehaviours = new();
            if (blocks.ContainsKey(pos))
            {
                blockBehaviours.AddRange(
                    from block in blocks[pos]
                    where block.GetComponent<T>() != null
                    select block.GetComponent<T>());
            }

            return blockBehaviours;
        }

        // TODO return block via out param?
        public bool Has<T>(Vector3Int pos) where T : BlockBehaviour
        {
            return Get<T>(pos) != null;
        }

        public bool HasOriented<T>(Vector3Int pos, Vector3Int orientation) where T : BlockBehaviour
        {
            var t = Get<T>(pos);
            return t != null && t.Block.Orientation == orientation;
        }

        public bool IsEmpty(Vector3Int pos)
        {
            return !Blocks.ContainsKey(pos);
        }

        public List<Triggerable> GetTriggers()
        {
            List<Triggerable> triggerables = new();
            foreach (List<Block> blockList in Blocks.Values)
            {
                triggerables.AddRange(
                    blockList.Select(block => block.GetComponent<Triggerable>())
                        .Where(triggerable => triggerable != null));
            }

            return triggerables;
        }

        public List<Movable> GetMovers()
        {
            List<Movable> movables = new();
            foreach (List<Block> blockList in Blocks.Values)
            {
                movables.AddRange(
                    blockList.Select(block => block.GetComponent<Movable>())
                        .Where(triggerable => triggerable != null));
            }

            return movables;
        }
    }
}