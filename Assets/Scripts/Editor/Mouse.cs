using UnityEditor;
using UnityEngine;

namespace GridGame.Editor
{
    public static class Mouse
    {
        public static Vector3Int GetPosition(Vector3 mousePos, bool avoidIntersect)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePos);

            if (Physics.Raycast(ray, out RaycastHit hit, 1000.0f))
            {
                Vector3 pos;
                if (avoidIntersect)
                {
                    pos = Utils.AvoidIntersect(hit.point + (hit.normal * 0.5f));
                }
                else
                {
                    pos = hit.transform.position;
                    if (Utils.TileIsEmpty(pos))
                    {
                        pos = hit.point;
                    }
                }

                return Vector3Int.RoundToInt(pos);
            }

            return Vector3Int.zero;
        }

        public static Vector3Int GetPositionOnPlane(Vector3 mousePos, Plane plane)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePos);

            if (plane.Raycast(ray, out float enter))
            {
                return Vector3Int.RoundToInt(ray.GetPoint(enter));
            }

            return Vector3Int.zero;
        }
    }
}