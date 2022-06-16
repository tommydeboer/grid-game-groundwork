using GridGame.Blocks;
using UnityEditor;
using UnityEngine;

namespace GridGame.Editor
{
    [CustomEditor(typeof(Block))]
    public class BlockEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            GUILayout.Space(10);
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);
            if (GUILayout.Button("Build Faces"))
            {
                Block myScript = (Block)target;
                myScript.BuildFaces();
            }
        }
    }
}