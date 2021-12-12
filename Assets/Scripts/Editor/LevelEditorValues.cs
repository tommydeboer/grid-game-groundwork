using System.Collections.Generic;
using UnityEngine;

namespace Editor
{
    public class LevelEditorValues : ScriptableObject
    {
        public int SpawnHeight { get; set; }
        public Vector3 SpawnRotation { get; set; }
        public Color GizmoColor { get; set; } = Color.white;
        public string CurrentLevel { get; set; }
        public PlacementMode PlacementMode { get; set; }
        public EditorPrefabs EditorPrefabs { get; set; }
        public List<GameObject> Prefabs => EditorPrefabs.Prefabs;
        public GameObject SelectedPrefab { get; private set; }

        public void SetPrefab(int index)
        {
            SelectedPrefab = Prefabs[index];
        }
    }
}