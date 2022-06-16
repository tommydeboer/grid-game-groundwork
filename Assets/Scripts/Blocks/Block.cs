using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace GridGame.Blocks
{
    public class Block : MonoBehaviour
    {
        [Header("Faces")]
        [SerializeField]
        GameObject topFace;

        [SerializeField]
        GameObject bottomFace;

        [SerializeField]
        GameObject frontFace;

        [SerializeField]
        GameObject backFace;

        [SerializeField]
        GameObject leftFace;

        [SerializeField]
        GameObject rightFace;

        [Header("Other settings")]
        [SerializeField]
        BlockMaterial material = BlockMaterial.Default;

        readonly Dictionary<Direction, Quaternion> rotationsFromTop = new()
        {
            { Direction.Up, Quaternion.Euler(0, 0, 0) },
            { Direction.Down, Quaternion.Euler(180, 0, 0) },
            { Direction.Left, Quaternion.Euler(0, 0, 90) },
            { Direction.Right, Quaternion.Euler(0, 0, -90) },
            { Direction.Forward, Quaternion.Euler(90, 0, 0) },
            { Direction.Back, Quaternion.Euler(-90, 0, 0) },
        };

        public void BuildFaces()
        {
            var faces = transform.Find("Faces");
            if (faces) DestroyImmediate(faces.gameObject);
            var facesParent = CreateFacesParent();

            Dictionary<Direction, GameObject> faceObjects = GetFaces();

            foreach ((Direction direction, GameObject facePrefab) in faceObjects)
            {
                GameObject go = PrefabUtility.InstantiatePrefab(facePrefab, facesParent.transform) as GameObject;
                Debug.Assert(go != null, "Couldn't instantiate face prefab");
                go.transform.position = transform.position;
                go.transform.rotation = rotationsFromTop[direction];
            }
        }

        Dictionary<Direction, GameObject> GetFaces()
        {
            Dictionary<Direction, GameObject> faceObjects = new();
            if (topFace) faceObjects[Direction.Up] = topFace;
            if (bottomFace) faceObjects[Direction.Down] = bottomFace;
            if (frontFace) faceObjects[Direction.Forward] = frontFace;
            if (backFace) faceObjects[Direction.Back] = backFace;
            if (leftFace) faceObjects[Direction.Left] = leftFace;
            if (rightFace) faceObjects[Direction.Right] = rightFace;
            return faceObjects;
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

        public BlockMaterial Material => material;
        public Vector3 Position => transform.position;
        public Vector3 Rotation => transform.eulerAngles;
        public Vector3Int Orientation => Vector3Int.RoundToInt(Quaternion.Euler(Rotation) * Vector3.back);
        public Vector3 Below => Position + Vector3.down;

        [CanBeNull]
        public Block GetNeighbour(Vector3Int direction)
        {
            if (Physics.Raycast(transform.position, direction, out RaycastHit hit, 1f, (int)Layers.GridPhysics))
            {
                return hit.collider.gameObject.GetComponentInParent<Block>();
            }

            return null;
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