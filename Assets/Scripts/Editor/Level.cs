using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace GridGame.Editor
{
    [Serializable]
    public class Level
    {
        [SerializeField]
        public string sceneName;

        Transform root;

        Transform Root
        {
            get
            {
                if (root == null)
                {
                    root = SceneManager.GetSceneByName(sceneName)
                        .GetRootGameObjects()
                        .ToList()
                        .Find(obj => obj.CompareTag(Tags.LEVEL))
                        .transform;

                    if (root == null)
                    {
                        Debug.LogError("Level scene doesn't contain a Level object");
                    }
                }

                return root;
            }
        }

        public Level(string sceneName)
        {
            this.sceneName = sceneName;
        }

        public void CreateAt(Object prefab, Vector3 pos, Vector3? eulerAngles = null)
        {
            Debug.Assert(prefab != null, "Attempted to create null block");

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
                        if (tile.CompareTag(Tags.TILE) && atPosition)
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