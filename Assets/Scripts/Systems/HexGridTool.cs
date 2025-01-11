using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(HexGridGenerator))]
[RequireComponent(typeof(HexPainter))]
[RequireComponent(typeof(HexCreator))]
[RequireComponent(typeof(HexWalkable))]
public class HexGridTool : BaseSignal
{
    public Hex HexPrefab;
    public EnumHexGridGenerationType GenerationType;
    public EnumHexGridMode Mode;
    public EnumTransformTool TransformTool;

    [Header("Hexagonal Grid Settings")]
    public int ArenaRadius;

    [Header("Square Grid Settings")]
    public int SquareWidth;
    public int SquareHeight;

    [Header("Paint Settings")]
    public List<HexPaintOption> HexPaintOptions;

    [Header("Transform Settings")]
    public int TargetHeight;
    public int TargetLenght;

    [Header("Generator Reference")]
    public HexGridGenerator HexGridGenerator;
    public HexPainter HexPainter;
    public HexCreator HexCreator;
    public HexWalkable HexWalkable;

    public bool IsEraseMode;

    public bool IsWalkable;

    Hex Hex;

    private void OnEnable()
    {
        HexGridGenerator = gameObject.GetComponent<HexGridGenerator>();
        HexPainter = gameObject.GetComponent<HexPainter>();
        HexCreator = gameObject.GetComponent<HexCreator>();
        HexWalkable = gameObject.GetComponent<HexWalkable>();

        HexWalkable.HideHexWalkableMap();

        #if UNITY_EDITOR
        if(Application.isPlaying) return;
        Selection.selectionChanged += OnSelectionChanged;
        SceneView.duringSceneGui += OnSceneGUI;
        #endif
    }

    private void OnDisable()
    {
        #if UNITY_EDITOR
        if(Application.isPlaying) return;
        Selection.selectionChanged -= OnSelectionChanged;
        SceneView.duringSceneGui -= OnSceneGUI;
        Mode = EnumHexGridMode.Generation;
        #endif
    }

    private void OnSelectionChanged()
    {
        if (Selection.activeObject != gameObject)
        {
            Mode = EnumHexGridMode.Generation;

            Hex = null;

            HexWalkable.HideHexWalkableMap();
        }
    }

    #if UNITY_EDITOR
    private void OnSceneGUI(SceneView sceneView)
    {
        if (Application.isPlaying || Selection.activeObject != gameObject) return;

        Event Event = Event.current;

        if(Hex) Hex.Enter.SetActive(false);

        Hex = HexRay(Event, out RaycastHit Hit);

        Hex?.Enter.SetActive(true);

        if (Hex?.Enter.activeSelf == true)
        {
            Handles.BeginGUI();
            Vector2 size = new Vector2(180, 150);

            int connectedHexesCount = Hex.ConnectedHexes != null ? Hex.ConnectedHexes.Count : 0;
            float dynamicHeight = 150 + connectedHexesCount * 16;
            Rect rect = new Rect(10, Screen.height - dynamicHeight - 10, size.x, dynamicHeight);

            GUI.Box(rect, GUIContent.none, EditorStyles.helpBox);
            GUILayout.BeginArea(rect);

            GUIStyle infoStyle = new GUIStyle(EditorStyles.label);
            infoStyle.normal.textColor = Color.white;
            infoStyle.fontStyle = FontStyle.Bold;

            GUILayout.Label(" Hex Information", infoStyle);

            GUILayout.Label($" Position: X: {Hex.Position.x}  Y: {Hex.Position.y}  Z: {Hex.Position.z}", infoStyle);
            GUILayout.Label($" Length: {Hex.Lenght}", infoStyle);
            GUILayout.Label($" IsWalkable: {(Hex.IsWalkable ? "Yes" : "No")}", infoStyle);

            GUILayout.Space(10);
            GUILayout.Label(" Connected Hexes", infoStyle);
            if (Hex.ConnectedHexes != null && Hex.ConnectedHexes.Count > 0)
            {
                foreach (var ConnectedHex in Hex.ConnectedHexes)
                {
                    string HexName = ConnectedHex.HexVisual != null ? ConnectedHex.HexVisual.name.Replace("(Clone)", "").Trim() : "Unnamed Hex";
                    GUILayout.Label($" {HexName} - X: {ConnectedHex.Position.x}, Y: {ConnectedHex.Position.y}, Z: {ConnectedHex.Position.z}", infoStyle);
                }
            }
            else
            {
                GUILayout.Label(" No connected hexes.", infoStyle);
            }
            GUILayout.EndArea();
            Handles.EndGUI();
        }

        switch (Mode)
        {
            case EnumHexGridMode.Paint :
                if(HexPaintOptions != null && HexPaintOptions.Count > 0 && (Event.type == EventType.MouseDown || Event.type == EventType.MouseDrag) && Event.button == 0)
                {
                    if(Hex)
                    {
                        if(IsEraseMode)
                        {
                            if(Event.type == EventType.MouseDown)
                            HexCreator.RemoveHex(Hex);
                        }
                        else
                        {
                            HexPainter.HandlePainting(Hex, HexPaintOptions);
                        }                  
                    }
                    Event.Use();
                }
                break;
            case EnumHexGridMode.Transform :
                if((Event.type == EventType.MouseDown || Event.type == EventType.MouseDrag) && Event.button == 0)
                {
                    switch (TransformTool)
                    {
                        case EnumTransformTool.SetHeight :
                            Hex?.SetHeight(TargetHeight);
                            break;
                        case EnumTransformTool.SetLenght :
                            Hex?.SetLength(TargetLenght);
                            break;
                        default: break;
                    }
                    Event.Use();                            
                }
                break;
            case EnumHexGridMode.Creation :
                if(HexPrefab && Event.type == EventType.MouseDown && Event.button == 0)
                {
                    if(Hex) HexCreator.CreateHexNeighbor(Hex, Hit, HexPrefab.gameObject, HexPaintOptions, TargetHeight, TargetLenght, SquareWidth, SquareHeight);
                    Event.Use();    
                }
                break;
            case EnumHexGridMode.SetWalkable :
                if((Event.type == EventType.MouseDown || Event.type == EventType.MouseDrag) && Event.button == 0)
                {
                    if(Hex)
                    {
                        Hex.SetIsWalkable(IsWalkable);
                        HexWalkable.SetWalkableMap(Hex);                   
                    }
                    Event.Use();    
                }                
                break;
            default: break;
        }
    }

    private Hex HexRay(Event Event, out RaycastHit Hit)
    {
        Hex Hex = null;
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Transform Parent = hit.collider.transform.parent;
            if (Parent != null && Parent.TryGetComponent<Hex>(out Hex CheckHex)) Hex = CheckHex;
        }

        Hit = hit;
        return Hex;
    }
    #endif
}