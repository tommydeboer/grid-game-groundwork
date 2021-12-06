using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class Level
    {
        readonly Transform root;

        public Transform Root => root;

        public Level(Transform root)
        {
            this.root = root;
        }

        public void CreateAt(Object prefab, Vector3 pos, Vector3? eulerAngles = null)
        {
            if (prefab == null)
            {
                // TODO this check should be moved to LevelEditor
                ClearAt(Vector3Int.RoundToInt(pos));
            }
            else
            {
                GameObject obj = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                if (obj == null)
                {
                    return;
                }

                obj.transform.position = pos;
                obj.transform.parent = Root;
                obj.transform.eulerAngles = eulerAngles ?? Vector3.zero;


                // TODO let LevelEditor pass a correct V3 to this method
                // Vector3 p = obj.transform.position;
                // if (spawnHeight < p.z)
                // {
                //     obj.transform.position = new Vector3(p.x, p.y, -Mathf.Abs(spawnHeight));
                // }

                Utils.AvoidIntersect(obj.transform);
                Undo.RegisterCreatedObjectUndo(obj, "Create object");
            }
        }

        public void ClearAt(Vector3Int pos)
        {
            bool foundSomething = true;
            while (foundSomething)
            {
                foundSomething = false;
                foreach (Transform child in root)
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