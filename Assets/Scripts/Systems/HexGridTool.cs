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
    public Hex Ground;
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

    private void OnEnable()
    {
        HexGridGenerator = gameObject.GetComponent<HexGridGenerator>();
        HexPainter = gameObject.GetComponent<HexPainter>();
        HexCreator = gameObject.GetComponent<HexCreator>();
        HexWalkable = gameObject.GetComponent<HexWalkable>();

        Selection.selectionChanged += OnSelectionChanged;
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        Selection.selectionChanged -= OnSelectionChanged;
        SceneView.duringSceneGui -= OnSceneGUI;
        Mode = EnumHexGridMode.Generation;
    }

    private void OnSelectionChanged()
    {
        if (Selection.activeObject != gameObject)
        {
            Mode = EnumHexGridMode.Generation;
            HexWalkable.HideHexWalkableMap();
        }
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (Application.isPlaying || Selection.activeObject != gameObject)
            return;

        Event Event = Event.current;

        switch (Mode)
        {
            case EnumHexGridMode.Paint :
                if(HexPaintOptions != null && HexPaintOptions.Count > 0 && (Event.type == EventType.MouseDown || Event.type == EventType.MouseDrag) && Event.button == 0)
                {
                    Hex Hex = HexRay(Event, out RaycastHit Hit);
                    if(Hex)
                    {
                        if(IsEraseMode)
                        {
                            if(Event.type == EventType.MouseDown)
                            HexCreator.RemoveHex(Hex);
                        }
                        else
                        {
                            HexPainter.HandlePainting(Hex, IsEraseMode, HexPaintOptions);
                        }                  
                    }
                    Event.Use();
                }
                break;
            case EnumHexGridMode.Transform :
                if((Event.type == EventType.MouseDown || Event.type == EventType.MouseDrag) && Event.button == 0)
                {
                    Hex Hex = HexRay(Event, out RaycastHit Hit);

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
                if(Ground && Event.type == EventType.MouseDown && Event.button == 0)
                {
                    Hex Hex = HexRay(Event, out RaycastHit Hit);
                    if(Hex) HexCreator.CreateHexNeighbor(Hex, Hit, Ground.gameObject, TargetHeight, TargetLenght, SquareWidth, SquareHeight);
                    Event.Use();    
                }
                break;
            case EnumHexGridMode.Walkable :
                if((Event.type == EventType.MouseDown || Event.type == EventType.MouseDrag) && Event.button == 0)
                {
                    Hex Hex = HexRay(Event, out RaycastHit Hit);
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
}