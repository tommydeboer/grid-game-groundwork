using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace GridGame.Blocks
{
    public class Block : MonoBehaviour
    {
        [Serializable]
        class FaceObject
        {
            public Direction direction;
            public GameObject prefab;
        }

        [Tooltip("Face prefabs for each side of the block")]
        [SerializeField]
        List<FaceObject> faces = new(6);

        [Tooltip("If true, empty faces will count as solid faces")]
        [SerializeField]
        bool isSolid;

        // TODO remove this concept when Player, Exit, etc. are entities instead of blocks
        [Tooltip("Non-full sized blocks fit inside full sized blocks")]
        [SerializeField]
        bool isFullSized;

        public bool IsDynamic { get; private set; }
        public bool IsFullSized => isFullSized;
        public bool IsSolid => isSolid;
        public Block AttachedTo { get; set; }
        public Vector3 Position => transform.position;
        public Vector3 Rotation => transform.eulerAngles;
        public Vector3Int Orientation => Vector3Int.RoundToInt(Quaternion.Euler(Rotation) * Vector3.back);
        public Vector3 Below => Position + Vector3.down;

#if UNITY_EDITOR
        public void BuildFaces()
        {
            var facesTransform = transform.Find("Faces");
            if (facesTransform) DestroyImmediate(facesTransform.gameObject);
            var facesParent = CreateFacesParent();

            foreach (var face in faces)
            {
                GameObject go = PrefabUtility.InstantiatePrefab(face.prefab, facesParent.transform) as GameObject;
                Debug.Assert(go != null, "Couldn't instantiate face prefab");
                go.transform.position = transform.position;
                go.transform.rotation = Direction.Up.RotateTo(face.direction);
            }
        }

        GameObject CreateFacesParent()
        {
            return new GameObject
            {
                name = "Faces",
                transform =
                {
                    parent = transform
                }
            };
        }
#endif

        void Awake()
        {
            IsDynamic = GetComponent<Movable>();
        }

        public bool HasFaceAt(Direction direction)
        {
            if (IsSolid) return true;
            return faces.Find(o => o.direction == direction) != null;
        }

        [CanBeNull]
        public Block GetNeighbour(Vector3Int direction)
        {
            return Utils.GetBlockAtPos(Position + direction);
        }

        [CanBeNull]
        public T GetNeighbouring<T>(Vector3Int direction) where T : BlockBehaviour
        {
            return GetNeighbour(direction)?.GetComponent<T>();
        }

        public bool HasNeighbouring<T>(Vector3Int direction) where T : BlockBehaviour
        {
            // TODO add an out parameter
            var neighbour = GetNeighbour(direction);
            return neighbour && neighbour.GetComponent<T>();
        }

        public bool HasNeighbouringOriented<T>(Vector3Int direction, Vector3Int orientation) where T : BlockBehaviour
        {
            // TODO add an out parameter
            var neighbour = GetNeighbour(direction);
            return neighbour && neighbour.GetComponent<T>() && neighbour.Orientation == orientation;
        }

        public bool HasEmptyAt(Vector3Int direction)
        {
            return GetNeighbour(direction) == null;
        }

        public bool Is<T>() where T : BlockBehaviour
        {
            return GetComponent<T>();
        }

        readonly Collider[] intersections = new Collider[3];

        public bool Intersects<T>() where T : BlockBehaviour
        {
            int hits = Physics.OverlapBoxNonAlloc(
                Position,
                Vector3.one * .49f,
                intersections,
                Quaternion.identity,
                (int)Layers.GridPhysics
            );

            for (int i = 0; i < hits; i++)
            {
                if (intersections[i].gameObject.GetComponentInParent<Block>() == this) continue;
                if (intersections[i].gameObject.GetComponentInParent<T>())
                {
                    return true;
                }
            }

            return false;
        }

        // TODO remove when GetNeighbour returns edge before block
        public bool IsOriented<T>(Vector3Int orientation) where T : BlockBehaviour
        {
            return GetComponent<T>() && Orientation == orientation;
        }
    }
}