using System.Collections.Generic;
using UnityEngine;

namespace GridGame.Editor
{
    public class EditorPrefabs : ScriptableObject
    {
        [SerializeField]
        List<GameObject> prefabs;

        public List<GameObject> Prefabs => prefabs;
    }
}