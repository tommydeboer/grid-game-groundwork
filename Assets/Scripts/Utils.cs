using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GridGame.Blocks;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GridGame
{
    public class Utils
    {
        public static List<Movable> movers = new();
        static int maxColliders = 5;

        public static IEnumerator LoadScene(string scene)
        {
            yield return WaitFor.EndOfFrame;
            SceneManager.LoadScene(scene, LoadSceneMode.Single);
        }

        public static int StringToInt(string intString)
        {
            int i = 0;
            if (!System.Int32.TryParse(intString, out i))
            {
                i = 0;
            }

            return i;
        }

        public static bool Roughly(float one, float two, float tolerance = 0.5f)
        {
            return Mathf.Abs(one - two) < tolerance;
        }

        public static bool VectorRoughly(Vector3 one, Vector3 two, float t = 0.5f)
        {
            return Roughly(one.x, two.x, t) && Roughly(one.y, two.y, t) && Roughly(one.z, two.z, t);
        }

        public static bool VectorRoughly2D(Vector3 one, Vector3 two, float t = 0.5f)
        {
            return Roughly(one.x, two.x, t) && Roughly(one.y, two.y, t);
        }

        public static void RoundPosition(Transform t)
        {
            Vector3 p = t.position;
            t.position = Vec3ToInt(p);
        }

        public static void AvoidIntersect(Transform root)
        {
            bool intersecting = true;
            while (intersecting)
            {
                intersecting = false;
                foreach (Transform tile in root)
                {
                    if (tile.gameObject.CompareTag(Tags.TILE))
                    {
                        Movable m = GetMoverAtPos(tile.position);
                        if (m != null && m.transform != root)
                        {
                            root.position += Vector3.back;
                            intersecting = true;
                        }
                        else
                        {
                            Static static_ = GetStaticAtPos(tile.position);
                            if (static_ != null && static_.transform != root)
                            {
                                root.position += Vector3.back;
                                intersecting = true;
                            }
                        }
                    }
                }
            }
        }

        public static Vector3 AvoidIntersect(Vector3 v)
        {
            bool intersecting = true;
            while (intersecting)
            {
                intersecting = false;

                if (!TileIsEmpty(v))
                {
                    v += Vector3.back;
                    intersecting = true;
                }
            }

            return v;
        }

        public static Vector3Int Vec3ToInt(Vector3 v)
        {
            return Vector3Int.RoundToInt(v);
        }

        public static bool TileIsEmpty(Vector3 pos)
        {
            return TileIsEmpty(Vec3ToInt(pos));
        }

        public static bool TileIsEmpty(Vector3Int pos)
        {
            return StaticIsAtPos(pos) == false && MoverIsAtPos(pos) == false;
        }

        public static Collider[] GetCollidersAt(Vector3 pos)
        {
            return GetCollidersAt(Vec3ToInt(pos));
        }

        public static Collider[] GetCollidersAt(Vector3Int pos)
        {
            Collider[] colliders = new Collider[maxColliders];
            int numColliders = Physics.OverlapSphereNonAlloc(pos, 0.4f, colliders);
            System.Array.Resize(ref colliders, numColliders);
            return colliders;
        }

        // WALLS // 

        public static Static GetStaticAtPos(Vector3Int pos)
        {
            Collider[] colliders = GetCollidersAt(pos);

            return colliders
                .Select(t => t.GetComponentInParent<Static>())
                .FirstOrDefault(static_ => static_ != null);
        }

        public static Static GetStaticAtPos(Vector3 pos)
        {
            return GetStaticAtPos(Vec3ToInt(pos));
        }

        public static bool StaticIsAtPos(Vector3 pos)
        {
            return StaticIsAtPos(Vec3ToInt(pos));
        }

        public static bool StaticIsAtPos(Vector3Int pos)
        {
            return GetStaticAtPos(pos) != null;
        }

        // MOVERS // 

        public static Movable GetMoverAtPos(Vector3 pos)
        {
            return GetMoverAtPos(Vec3ToInt(pos));
        }

        [CanBeNull]
        public static Block GetBlockAtPos(Vector3 pos)
        {
            Collider[] colliders = GetCollidersAt(pos);

            return colliders
                .Select(collider => collider.GetComponentInParent<Block>())
                .FirstOrDefault(block => block != null);
        }

        public static Movable GetMoverAtPos(Vector3Int pos)
        {
            Collider[] colliders = GetCollidersAt(pos);

            return colliders
                .Select(t => t.GetComponentInParent<Movable>())
                .FirstOrDefault(m => m != null);
        }

        public static bool MoverIsAtPos(Vector3 pos)
        {
            return MoverIsAtPos(Vec3ToInt(pos));
        }

        public static bool MoverIsAtPos(Vector3Int pos)
        {
            return GetMoverAtPos(pos) != null;
        }
    }
}