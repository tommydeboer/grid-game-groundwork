using System.Collections.Generic;
using UnityEngine;

namespace Editor
{
    public class EditorPrefabs : ScriptableObject
    {
        [SerializeField]
        List<GameObject> prefabs;

        public List<GameObject> Prefabs => prefabs;
    }
}