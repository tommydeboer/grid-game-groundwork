using System;
using GridGame.Blocks;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

namespace GridGame.Grid
{
    public abstract record GridAnimation
    {
        public Movable Movable { get; init; }

        public abstract void Play(float moveTime, Action<GridAnimation> finishCallback);
    }
}