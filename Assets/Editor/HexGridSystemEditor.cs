#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(HexGridSystem))]
public class HexGridSystemEditor : Editor
{
    private bool isEraseModeActive = false;

    public override void OnInspectorGUI()
    {
        if (Application.isPlaying)
        {
            EditorGUILayout.HelpBox("HexGridSystemEditor не доступен во время воспроизведения.", MessageType.Warning);
            return;
        }

        HexGridSystem hexGridSystem = (HexGridSystem)target;

        Undo.RecordObject(hexGridSystem, "Modify HexGridSystem");

        hexGridSystem.Mode = (EnumHexGridMode)EditorGUILayout.EnumPopup("Mode", hexGridSystem.Mode);

        EditorGUILayout.Space();

        switch (hexGridSystem.Mode)
        {
            case EnumHexGridMode.Generation :

                hexGridSystem.GenerationType = (EnumHexGridGenerationType)EditorGUILayout.EnumPopup("Generation Type", hexGridSystem.GenerationType);

                switch (hexGridSystem.GenerationType)
                {
                    case EnumHexGridGenerationType.Hexagonal:
                        hexGridSystem.ArenaRadius = EditorGUILayout.IntField("Arena Radius", hexGridSystem.ArenaRadius);
                        hexGridSystem.SquareWidth = 0;
                        hexGridSystem.SquareHeight = 0;
                        break;

                    case EnumHexGridGenerationType.Square:
                        hexGridSystem.SquareWidth = EditorGUILayout.IntField("Square Width", hexGridSystem.SquareWidth);
                        hexGridSystem.SquareHeight = EditorGUILayout.IntField("Square Height", hexGridSystem.SquareHeight);
                        break;

                    default: break;
                }

                hexGridSystem.Ground = (Hex)EditorGUILayout.ObjectField("Ground Prefab", hexGridSystem.Ground, typeof(Hex), false);

                hexGridSystem.TargetHeight = EditorGUILayout.IntField("Target Height", Mathf.Clamp(hexGridSystem.TargetHeight, 1, 10));

                if (GUILayout.Button("Generate Hex Grid"))
                {
                    switch (hexGridSystem.GenerationType)
                    {
                        case EnumHexGridGenerationType.Hexagonal :
                            hexGridSystem.HexGridGenerator.GenerateHexagonalGrid(hexGridSystem.ArenaRadius, hexGridSystem.TargetHeight, hexGridSystem.Ground);
                            break;
                        case EnumHexGridGenerationType.Square :
                            hexGridSystem.HexGridGenerator.GenerateHexagonalGridByDimensions(hexGridSystem.SquareHeight, hexGridSystem.SquareWidth, hexGridSystem.TargetHeight, hexGridSystem.Ground);
                            break;
                        default: break;
                    }
                    EditorUtility.SetDirty(hexGridSystem);
                }

                if (GUILayout.Button("Clear Hex Grid"))
                {
                    hexGridSystem.HexGridGenerator.ClearHexGrid();
                    EditorUtility.SetDirty(hexGridSystem);
                }
                break;
            case EnumHexGridMode.Paint :

                EditorGUILayout.LabelField("Paint Settings", EditorStyles.boldLabel);

                if (hexGridSystem.HexPaintOptions == null) hexGridSystem.HexPaintOptions = new List<HexPaintOption>();

                EditorGUILayout.Space();

                Color originalColor = GUI.backgroundColor;
                GUI.backgroundColor = isEraseModeActive ? Color.green : originalColor;

                if (GUILayout.Button("Erase Mode"))
                {
                    isEraseModeActive = !isEraseModeActive;
                    hexGridSystem.IsEraseMode = isEraseModeActive;
                    DeselectAllOptions(hexGridSystem.HexPaintOptions);
                }

                GUI.backgroundColor = originalColor;

                for (int i = 0; i < hexGridSystem.HexPaintOptions.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();

                    string buttonText = hexGridSystem.HexPaintOptions[i].HexPrefab != null ? "Select" : "None";
                    GUI.backgroundColor = hexGridSystem.HexPaintOptions[i].IsSelected ? Color.green : originalColor;

                    if (GUILayout.Button(buttonText, GUILayout.Width(80)))
                    {
                        isEraseModeActive = false;
                        hexGridSystem.IsEraseMode = false;
                        SelectOption(hexGridSystem.HexPaintOptions, i);
                    }

                    GUI.backgroundColor = originalColor;

                    GUILayout.FlexibleSpace();
                    hexGridSystem.HexPaintOptions[i].HexPrefab = (MeshRenderer)EditorGUILayout.ObjectField("", hexGridSystem.HexPaintOptions[i].HexPrefab, typeof(MeshRenderer), false, GUILayout.ExpandWidth(true));

                    if (GUILayout.Button("Remove", GUILayout.Width(80)))
                    {
                        hexGridSystem.HexPaintOptions.RemoveAt(i);
                        EditorUtility.SetDirty(hexGridSystem);
                        break;
                    }

                    EditorGUILayout.EndHorizontal();
                }

                if (GUILayout.Button("Add Hex Paint Option"))
                {
                    hexGridSystem.HexPaintOptions.Add(new HexPaintOption());
                    EditorUtility.SetDirty(hexGridSystem);
                }            
                break;
            case EnumHexGridMode.Transform :

                EditorGUILayout.LabelField("Transform Settings", EditorStyles.boldLabel);

                hexGridSystem.TransformTool = (EnumTransformTool)EditorGUILayout.EnumPopup("Transform Tool", hexGridSystem.TransformTool);

                if (hexGridSystem.TransformTool == EnumTransformTool.SetHeight)
                {
                    hexGridSystem.TargetHeight = EditorGUILayout.IntField("Target Height", Mathf.Clamp(hexGridSystem.TargetHeight, 1, 10));
                }            
                break;
            case EnumHexGridMode.Bridge :

                hexGridSystem.Ground = (Hex)EditorGUILayout.ObjectField("Ground Prefab", hexGridSystem.Ground, typeof(Hex), false);
                hexGridSystem.TargetHeight = EditorGUILayout.IntField("Target Height", Mathf.Clamp(hexGridSystem.TargetHeight, 1, 10));

                Color originalButtonColor = GUI.backgroundColor;
                GUI.backgroundColor = isEraseModeActive ? Color.green : originalButtonColor;

                if (GUILayout.Button("Erase Mode"))
                {
                    isEraseModeActive = !isEraseModeActive;
                    hexGridSystem.IsEraseMode = isEraseModeActive;
                    DeselectAllOptions(hexGridSystem.HexPaintOptions);
                }

                break;
            default: break;
        }

        if (GUI.changed) EditorUtility.SetDirty(hexGridSystem);
    }

    private void SelectOption(List<HexPaintOption> options, int selectedIndex)
    {
        for (int i = 0; i < options.Count; i++)
        {
            options[i].IsSelected = i == selectedIndex;
        }
    }

    private void DeselectAllOptions(List<HexPaintOption> options)
    {
        foreach (var option in options)
        {
            option.IsSelected = false;
        }
    }
}
#endif