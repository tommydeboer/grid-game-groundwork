using System;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class InputDialog : EditorWindow
    {
        const string TextInput = "TextInputField";

        string inputText = "";
        string header;
        string okText;
        string prefix;
        Action<string> action;

        public static void ShowDialog(string title, string okText, Action<string> action, string prefix = null)
        {
            InputDialog dialog = CreateInstance<InputDialog>();
            dialog.header = title;
            dialog.okText = okText;
            dialog.action = action;
            dialog.prefix = prefix;
            dialog.maxSize = new Vector2(300, 70);
            dialog.CenterOnMainWin();
            dialog.ShowPopup();
        }

        void OnGUI()
        {
            EditorGUILayout.LabelField(header);

            using (new GUILayout.HorizontalScope())
            {
                if (prefix != null) GUILayout.Label(prefix);
                GUI.SetNextControlName(TextInput);
                inputText = EditorGUILayout.TextField(inputText);
                GUI.FocusControl(TextInput);
            }

            GUILayout.Space(5);

            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button(okText) && inputText != null)
                {
                    if (prefix != null)
                    {
                        action(prefix + inputText);
                    }
                    else
                    {
                        action(inputText);
                    }

                    Close();
                }

                if (GUILayout.Button("Cancel")) Close();
            }
        }

        void CenterOnMainWin()
        {
            Rect main = EditorGUIUtility.GetMainWindowPosition();
            Rect pos = position;
            float centerWidth = (main.width - pos.width) * 0.5f;
            float centerHeight = (main.height - pos.height) * 0.5f;
            pos.x = main.x + centerWidth;
            pos.y = main.y + centerHeight;
            position = pos;
        }
    }
}