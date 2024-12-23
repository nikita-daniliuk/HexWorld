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
        EditorGUILayout.LabelField("Position", $"X:  {Hex.Position.x}  Y:  {Hex.Position.z}  H:  {Hex.Position.y}");
        EditorGUILayout.LabelField("Is Busy", Hex.IsBisy ? "Yes" : "No");

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Connected Hexes", EditorStyles.boldLabel);
        if (Hex.ConnectedHexes != null && Hex.ConnectedHexes.Count > 0)
        {
            foreach (var ConnectedHex in Hex.ConnectedHexes)
            {
                string HexName = ConnectedHex.HexVisual != null ? ConnectedHex.HexVisual.name.Replace("(Clone)", "").Trim() : "Unnamed Hex";
                EditorGUILayout.LabelField($"{HexName}   X:  {ConnectedHex.Position.x}  Y:  {ConnectedHex.Position.z}  H:  {ConnectedHex.Position.y}");
            }
        }
        else
        {
            EditorGUILayout.LabelField("No connected hexes.");
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Emblems", EditorStyles.boldLabel);
        Hex.Pick = (GameObject)EditorGUILayout.ObjectField("Pick", Hex.Pick, typeof(GameObject), true);
        Hex.Enter = (GameObject)EditorGUILayout.ObjectField("Enter", Hex.Enter, typeof(GameObject), true);
        Hex.HexVisual = (MeshRenderer)EditorGUILayout.ObjectField("Hex Visual", Hex.HexVisual, typeof(MeshRenderer), true);

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }
}
#endif