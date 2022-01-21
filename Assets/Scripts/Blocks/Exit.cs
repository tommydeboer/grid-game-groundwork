using Events;
using Player;
using UnityEditor;
using UnityEngine;

namespace Blocks
{
    public class Exit : Trigger
    {
        [SerializeField]
        SceneAsset levelToLoad;

        [SerializeField]
        LoadEventChannelSO loadChannel;

        public override BlockType Type => BlockType.Exit;

        public override void Check()
        {
            if (Grid.Has<PlayerInput>(Tile.gridPos))
            {
                // loadChannel.RaiseEvent(levelToLoad, false);
            }
        }
    }
}