using GridGame.Events;
using GridGame.Player;
using UnityEngine;

namespace GridGame.Blocks
{
    public class Exit : Trigger
    {
        [SerializeField]
        SceneField levelToLoad;

        [SerializeField]
        LoadEventChannelSO loadChannel;

        public override BlockType Type => BlockType.Exit;

        protected override void Start()
        {
            base.Start();
            Debug.Assert(levelToLoad != null, "Exit misses target level", gameObject);
            Debug.Assert(loadChannel != null, "Exit has no channel to broadcast to", gameObject);
        }

        public override void Check()
        {
            if (grid.Has<Hero>(Tile.gridPos))
            {
                loadChannel.RaiseSceneLoadRequestEvent(levelToLoad, false);
            }
        }
    }
}