using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GridGame.Editor
{
    public static class Draw
    {
        public static void DrawPrefabPreview(Vector3Int pos, Vector3 rot, GameObject prefab)
        {
            Matrix4x4 poseToWorld = Matrix4x4.TRS(pos, Quaternion.Euler(rot), Vector3.one);
            MeshFilter[] filters = prefab.GetComponentsInChildren<MeshFilter>();
            foreach (MeshFilter filter in filters)
            {
                Material mat = filter.GetComponent<MeshRenderer>().sharedMaterial;
                if (mat != null)
                {
                    Matrix4x4 childToPose = filter.transform.localToWorldMatrix;
                    Matrix4x4 childToWorld = poseToWorld * childToPose;
                    Mesh mesh = filter.sharedMesh;
                    mat.SetPass(0);
                    Graphics.DrawMesh(mesh, childToWorld, mat, 0);
                }
            }
        }

        public static void DrawRedOverlayBox(Bounds bounds)
        {
            DrawRedOverlayBox(bounds.center, bounds.size);
        }

        public static void DrawRedOverlayBox(Vector3 center, Vector3 size)
        {
            Matrix4x4 poseToWorld = Matrix4x4.TRS(center, Quaternion.identity, size);
            MeshFilter filter = EditorAssets.SelectionOverlay.GetComponentInChildren<MeshFilter>();
            Material mat = filter.GetComponent<MeshRenderer>().sharedMaterial;
            if (mat != null)
            {
                Mesh mesh = filter.sharedMesh;
                mat.SetPass(0);
                Graphics.DrawMesh(mesh, poseToWorld, mat, 0);
            }
        }

        public static void DrawWireCube(Vector3Int pos, Color color)
        {
            Handles.color = color;
            Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
            Handles.DrawWireCube(pos, Vector3.one);
            Handles.DrawWireCube(pos, Vector3.one * 1.01f);
            Handles.DrawWireCube(pos, Vector3.one * 0.99f);
        }

        public static void DrawWireBox(Bounds bounds, Color color, float width = 6f)
        {
            var minPos = bounds.min;
            var maxPos = bounds.max;

            var c1 = new Vector3(minPos.x, minPos.y, maxPos.z);
            var c2 = new Vector3(minPos.x, maxPos.y, maxPos.z);
            var c3 = new Vector3(minPos.x, maxPos.y, minPos.z);
            var c4 = new Vector3(maxPos.x, minPos.y, maxPos.z);
            var c5 = new Vector3(maxPos.x, minPos.y, minPos.z);
            var c6 = new Vector3(maxPos.x, maxPos.y, minPos.z);

            Handles.color = color;
            Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;

            Handles.DrawAAPolyLine(width, minPos, c1);
            Handles.DrawAAPolyLine(width, minPos, c3);
            Handles.DrawAAPolyLine(width, minPos, c5);
            Handles.DrawAAPolyLine(width, maxPos, c6);
            Handles.DrawAAPolyLine(width, maxPos, c4);
            Handles.DrawAAPolyLine(width, maxPos, c2);
            Handles.DrawAAPolyLine(width, c1, c2);
            Handles.DrawAAPolyLine(width, c4, c5);
            Handles.DrawAAPolyLine(width, c1, c4);
            Handles.DrawAAPolyLine(width, c3, c6);
            Handles.DrawAAPolyLine(width, c5, c6);
            Handles.DrawAAPolyLine(width, c3, c2);
        }

        public static void DrawHeightIndicator(Vector3Int pos, int height)
        {
            if (height == 0) return;

            for (int y = 1; y <= height; y++)
            {
                Handles.DrawWireCube(pos - (Vector3.up * y), Vector3.one);
            }
        }
    }
}