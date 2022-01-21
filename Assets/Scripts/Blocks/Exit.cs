using Events;
using UnityEditor;
using UnityEngine;

namespace Blocks
{
    public class Exit : Block
    {
        [SerializeField]
        SceneAsset levelToLoad;

        [SerializeField]
        LoadEventChannelSO loadChannel;

        public override BlockType Type => BlockType.Exit;
    }
}