using System.Collections.Generic;
using UnityEngine;

namespace GridGame.Editor
{
    public class State : ScriptableObject
    {
        public const int MAX_SPAWN_HEIGHT = 20;

        int spawnHeight;

        public int SpawnHeight
        {
            get => spawnHeight;
            set
            {
                if (0 <= value && value <= MAX_SPAWN_HEIGHT)
                {
                    spawnHeight = value;
                }
            }
        }

        public Vector3 SpawnRotation { get; set; }

        public void IncreaseRotation()
        {
            float y = SpawnRotation.y;
            y = y == 270f ? 0 : y + 90;
            SpawnRotation = new Vector3(SpawnRotation.x, y, SpawnRotation.z);
        }

        public void DecreaseRotation()
        {
            float y = SpawnRotation.y;
            y = y == 0 ? 270 : y - 90;
            SpawnRotation = new Vector3(SpawnRotation.x, y, SpawnRotation.z);
        }

        public Level CurrentLevel { get; set; }
        public EditorPrefabs EditorPrefabs { get; set; }
        public List<GameObject> Prefabs => EditorPrefabs.Prefabs;
        public int SelectedPrefabId { get; set; }
        public GameObject SelectedPrefab => Prefabs[SelectedPrefabId];

        public Mode Mode { get; set; }

        public void SetNextMode()
        {
            if (Mode == Mode.Create)
            {
                Mode = Mode.Erase;
            }
            else
            {
                Mode = Mode.Create;
            }
        }
    }
}