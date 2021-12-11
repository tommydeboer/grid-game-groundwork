using System.Collections.Generic;
using UnityEngine;

namespace Editor
{
    public class LevelEditorValues : ScriptableObject
    {
        public int SelectedPrefabId { get; set; }
        public int SpawnHeight { get; set; }
        public Color GizmoColor { get; set; } = Color.white;
        public string CurrentLevel { get; set; }
        public EditorPrefabs EditorPrefabs { get; set; }
        public List<GameObject> Prefabs => EditorPrefabs.Prefabs;
    }
}