using Events;
using Player;
using UnityEditor;
using UnityEngine;

namespace Blocks
{
    public class Exit : Trigger
    {
        [SerializeField]
        SceneField levelToLoad;

        [SerializeField]
        LoadEventChannelSO loadChannel;

        public override BlockType Type => BlockType.Exit;

        void Start()
        {
            Debug.Assert(levelToLoad != null, "Exit misses target level", gameObject);
            Debug.Assert(loadChannel != null, "Exit has no channel to broadcast to", gameObject);
        }

        public override void Check()
        {
            if (Grid.Has<PlayerInput>(Tile.gridPos))
            {
                loadChannel.RaiseSceneLoadRequestEvent(levelToLoad, false);
            }
        }
    }
}