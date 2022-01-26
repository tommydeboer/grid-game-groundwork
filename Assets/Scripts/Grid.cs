using System.Collections.Generic;
using System.Linq;
using GridGame.Blocks;
using GridGame.Events;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GridGame
{
    public class Grid : MonoBehaviour
    {
        [SerializeField]
        LoadEventChannelSO loadEventChannel;

        Dictionary<Vector3Int, Wall> walls;
        Dictionary<Vector3Int, Mover> movers;
        Dictionary<Vector3Int, Trigger> triggers;

        Dictionary<Vector3Int, Wall> Walls
        {
            get
            {
                if (walls == null) Reset();
                return walls;
            }
            set => walls = value;
        }

        Dictionary<Vector3Int, Mover> Movers
        {
            get
            {
                if (movers == null) Reset();
                return movers;
            }
            set => movers = value;
        }

        Dictionary<Vector3Int, Trigger> Triggers
        {
            get
            {
                if (triggers == null) Reset();
                return triggers;
            }
            set => triggers = value;
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
            Transform levelRoot = scene.GetRootGameObjects().First(go => go.CompareTag("Level")).transform;

            Walls = new Dictionary<Vector3Int, Wall>();
            Movers = new Dictionary<Vector3Int, Mover>();
            Triggers = new Dictionary<Vector3Int, Trigger>();

            foreach (Transform item in levelRoot)
            {
                var block = item.GetComponentInParent<Block>();
                switch (block)
                {
                    case Wall wall:
                        Walls[wall.Tile.gridPos] = wall;
                        break;
                    case Mover mover:
                        Movers[mover.Tile.gridPos] = mover;
                        break;
                    case Trigger trigger:
                        Triggers[trigger.Tile.gridPos] = trigger;
                        break;
                }
            }
        }

        public void Refresh()
        {
            var allMovers = Movers.Values;
            Movers = new Dictionary<Vector3Int, Mover>();

            foreach (Mover mover in allMovers)
            {
                Movers[mover.Tile.gridPos] = mover;
            }
        }

        public T Get<T>(Vector3Int pos) where T : Block
        {
            if (typeof(Wall).IsAssignableFrom(typeof(T)))
            {
                if (Walls.ContainsKey(pos) && Walls[pos] is T t)
                {
                    return t;
                }
            }
            else if (typeof(Mover).IsAssignableFrom(typeof(T)))
            {
                if (Movers.ContainsKey(pos) && Movers[pos] is T t)
                {
                    return t;
                }
            }

            return null;
        }

        // TODO return block via out param?
        public bool Has<T>(Vector3Int pos) where T : Block
        {
            return Get<T>(pos) != null;
        }

        public bool HasOriented<T>(Vector3Int pos, Vector3Int orientation) where T : Block
        {
            var block = Get<T>(pos);
            return block != null && block.Orientation == orientation;
        }

        public bool IsEmpty(Vector3Int pos)
        {
            return !Walls.ContainsKey(pos) && !Movers.ContainsKey(pos);
        }

        public List<Trigger> GetTriggers()
        {
            return Triggers.Values.ToList();
        }

        public List<Mover> GetMovers()
        {
            return Movers.Values.ToList();
        }
    }
}