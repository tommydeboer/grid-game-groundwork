using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class Level
    {
        Transform Root { get; }

        public Level(Transform root)
        {
            Root = root;
        }

        public void CreateAt(Object prefab, Vector3 pos, Vector3? eulerAngles = null)
        {
            if (prefab == null)
            {
                Debug.LogWarning("Attempted to create null object");
                return;
            }

            GameObject obj = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (obj == null)
            {
                return;
            }

            obj.transform.position = pos;
            obj.transform.parent = Root;
            obj.transform.eulerAngles = eulerAngles ?? Vector3.zero;

            Utils.AvoidIntersect(obj.transform);
            Undo.RegisterCreatedObjectUndo(obj, "Create object");
        }

        public void ClearAt(Vector3Int pos)
        {
            bool foundSomething = true;
            while (foundSomething)
            {
                foundSomething = false;
                foreach (Transform child in Root)
                {
                    foreach (Transform tile in child)
                    {
                        var position1 = tile.position;
                        bool atPosition = Utils.VectorRoughly(position1, pos);
                        if (tile.CompareTag("Tile") && atPosition)
                        {
                            foundSomething = true;
                            Undo.DestroyObjectImmediate(child.gameObject);
                            break;
                        }
                    }
                }
            }
        }
    }
}