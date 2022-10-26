using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;

namespace GridGame.Blocks
{
    public class Block : GridElement
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

        public bool IsSolid => isSolid;

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

        public bool HasFaceAt(Direction direction)
        {
            if (IsSolid) return true;
            var correctedDirection = Quaternion.Inverse(transform.rotation) * direction.AsVector();
            return faces.Find(o => o.direction == correctedDirection.ToDirection()) != null;
        }

        /// Blocks are grounded if there is a non-moving block beneath them.
        /// Entities are ignored: blocks fall on top of them.
        public override bool IsGrounded()
        {
            Block below = GetNeighbour(Direction.Down);
            if (!below) return false;

            Movable movableBelow = below.GetComponent<Movable>();
            if (movableBelow && movableBelow.IsFalling)
            {
                return false;
            }

            return true;
        }
    }
}