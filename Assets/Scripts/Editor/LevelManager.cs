using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

namespace Editor
{
    public static class LevelManager
    {
        public static List<KeyValuePair<string, string>> GetLevelScenes()
        {
            return AssetDatabase.FindAssets("t:Scene")
                .ToList()
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(path => new KeyValuePair<string, string>(path, Path.GetFileNameWithoutExtension(path)))
                .Where(level => level.Value.StartsWith("Level_"))
                .ToList();
        }
    }
}