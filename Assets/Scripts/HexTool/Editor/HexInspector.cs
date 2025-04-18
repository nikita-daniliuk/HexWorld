#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Hex))]
public class HexInspector : Editor
{
    public override void OnInspectorGUI()
    {
        Hex Hex = (Hex)target;

        EditorGUILayout.LabelField("Hex Information", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Position", $"X:  {Hex.Position.x}  Y:  {Hex.Position.y}  Z:  {Hex.Position.z}");
        EditorGUILayout.LabelField("Lenght", $"{Hex.Lenght}");
        EditorGUILayout.LabelField("IsWalkable", Hex.IsWalkable ? "Yes" : "No");

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Connected Hexes", EditorStyles.boldLabel);

        if (Hex.ConnectedHexes != null && Hex.ConnectedHexes.Count > 0)
        {
            foreach (var ConnectedHex in Hex.ConnectedHexes)
            {
                string HexName = ConnectedHex.HexVisual != null ? ConnectedHex.HexVisual.name.Replace("(Clone)", "").Trim() : "Unnamed Hex";
                EditorGUILayout.LabelField($"{HexName}   X:  {ConnectedHex.Position.x}  Y:  {ConnectedHex.Position.y}  H:  {ConnectedHex.Position.z}");
            }
        }
        else
        {
            EditorGUILayout.LabelField("No connected hexes.");
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Emblems", EditorStyles.boldLabel);
        Hex.HexVisual = (MeshRenderer)EditorGUILayout.ObjectField("Hex Visual", Hex.HexVisual, typeof(MeshRenderer), true);
        EditorGUILayout.Space();
        Hex.Pick = (GameObject)EditorGUILayout.ObjectField("Pick", Hex.Pick, typeof(GameObject), true);
        Hex.Enter = (GameObject)EditorGUILayout.ObjectField("Enter", Hex.Enter, typeof(GameObject), true);
        EditorGUILayout.Space();
        Hex.Walkable = (GameObject)EditorGUILayout.ObjectField("Walkable", Hex.Walkable, typeof(GameObject), true);
        Hex.NotWalkable = (GameObject)EditorGUILayout.ObjectField("NotWalkable", Hex.NotWalkable , typeof(GameObject), true);

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }
}
#endif