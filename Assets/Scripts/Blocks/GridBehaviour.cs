using System;
using UnityEngine;

namespace GridGame.Blocks
{
    public class GridBehaviour : MonoBehaviour
    {
        protected GridElement GridElement { get; private set; }

        protected virtual void Awake()
        {
            GridElement = GetComponent<GridElement>();
        }
    }
}