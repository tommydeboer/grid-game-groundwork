using UnityEngine;

public class LevelGizmo : MonoBehaviour
{
    static Vector3 pos { get; set; }
    static Color color = new(2f, 2f, 2f);
    static bool DrawEnabled { get; set; }

    public static void UpdateGizmo(Vector3 v, Color c)
    {
        pos = v;
        color = c;
    }

    public static void Enable(bool b)
    {
        DrawEnabled = b;
    }

    void OnDrawGizmos()
    {
        if (DrawEnabled)
        {
            Gizmos.color = color;
            Gizmos.DrawWireCube(pos, Vector3.one);
            Gizmos.DrawWireCube(pos + (Vector3.one * 0.01f), Vector3.one);
            Gizmos.DrawWireCube(pos - (Vector3.one * 0.01f), Vector3.one);
        }
    }
}