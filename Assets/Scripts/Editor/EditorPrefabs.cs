using System.Collections.Generic;
using UnityEngine;

namespace Editor
{
    [CreateAssetMenu]
    public class EditorPrefabs : ScriptableObject
    {
        [SerializeField]
        List<GameObject> prefabs;

        public List<GameObject> Prefabs => prefabs;
    }
}