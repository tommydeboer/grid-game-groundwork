using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class LevelFactory
    {
        public Level GetLevel(string name)
        {
            GameObject l = FindOrCreate("Level");
            l.tag = "Level";
            return new Level(l.transform);
        }

        static GameObject FindOrCreate(string s, Transform parentObj = null)
        {
            GameObject go = GameObject.Find(s);
            if (go == null)
            {
                go = new GameObject();
                go.transform.name = s;
                if (parentObj != null)
                {
                    go.transform.SetParent(parentObj);
                }

                Undo.RegisterCreatedObjectUndo(go, "Create object");
            }

            return go;
        }
    }
}