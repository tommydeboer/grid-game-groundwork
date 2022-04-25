using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace GridGame.Blocks
{
    public class Block : MonoBehaviour
    {
        //TODO should be moved to Movable? what strategy does a ladder have for example? Ladder won't see a Frame with current implementation
        [SerializeField]
        QueryStrategy gridQueryStrategy = QueryStrategy.CENTER_RAY;

        [SerializeField]
        BlockMaterial material = BlockMaterial.Default;

        public BlockMaterial Material => material;
        public Vector3 Position => transform.position;
        public Vector3 Rotation => transform.eulerAngles;
        public Vector3Int Orientation => Vector3Int.RoundToInt(Quaternion.Euler(Rotation) * Vector3.back);
        public Vector3 Below => Position + Vector3.down;

        enum QueryStrategy
        {
            CENTER_RAY,
            EDGE_RAYS,
            FULL_SURFACE
        }

        [CanBeNull]
        public Block GetNeighbour(Vector3 direction)
        {
            return Query(direction);
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

        [CanBeNull]
        Block Query(Vector3 direction)
        {
            return gridQueryStrategy switch
            {
                QueryStrategy.CENTER_RAY => QueryCenterRay(direction),
                QueryStrategy.EDGE_RAYS => QueryEdgeRays(direction),
                QueryStrategy.FULL_SURFACE => throw new NotImplementedException(),
                _ => throw new ArgumentOutOfRangeException(nameof(gridQueryStrategy), gridQueryStrategy, null)
            };
        }

        [CanBeNull]
        Block QueryCenterRay(Vector3 direction)
        {
            return DoSphereCast(transform.position, direction, .1f, 1f);
        }

        [CanBeNull]
        Block QueryEdgeRays(Vector3 direction)
        {
            Vector3 offset = direction.IsVertical() ? Vector3.right : Vector3.up;
            offset *= .45f;

            for (int i = 0; i < 4; i++)
            {
                Vector3 relativePos = Quaternion.AngleAxis(90 * i, direction) * offset;
                Vector3 rayOrigin = transform.position + relativePos + direction * 0.45f;
                Block block = DoRayCast(rayOrigin, direction, .8f);
                if (block) return block;
            }

            return null;
        }

        [CanBeNull]
        static Block DoRayCast(Vector3 origin, Vector3 direction, float distance)
        {
            if (Physics.Raycast(origin, direction, out RaycastHit hit, distance, (int)Layers.GridPhysics))
            {
                return hit.collider.gameObject.GetComponentInParent<Block>();
            }

            return null;
        }

        [CanBeNull]
        static Block DoSphereCast(Vector3 origin, Vector3 direction, float radius, float distance)
        {
            if (Physics.SphereCast(origin, radius, direction, out RaycastHit hit, distance, (int)Layers.GridPhysics))
            {
                return hit.collider.gameObject.GetComponentInParent<Block>();
            }

            return null;
        }

        void OnDrawGizmosSelected()
        {
            Vector3 pos = transform.position;
            Gizmos.color = Color.red;

            Vector3 direction = Vector3.right;
            Vector3 offset = direction.IsVertical() ? Vector3.right : Vector3.up;
            offset *= .45f;

            for (int i = 0; i < 4; i++)
            {
                Vector3 rayOrigin = Quaternion.AngleAxis(90 * i, direction) * offset;

                Vector3 relPos = pos + rayOrigin + direction * 0.45f;
                Gizmos.DrawLine(relPos, relPos + direction * .8f);
            }
        }
    }
}