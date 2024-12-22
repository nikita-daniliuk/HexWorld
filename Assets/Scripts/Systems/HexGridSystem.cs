using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(HexGridGenerator))]
[RequireComponent(typeof(HexPainter))]
[RequireComponent(typeof(HexBridgeGenerator))]
public class HexGridSystem : BaseSignal
{
    [SerializeField] public Hex Ground;
    [SerializeField] public EnumHexGridGenerationType GenerationType;
    [SerializeField] public EnumHexGridMode Mode;
    [SerializeField] public EnumTransformTool TransformTool;

    [Header("Hexagonal Grid Settings")]
    [SerializeField] public int ArenaRadius;

    [Header("Square Grid Settings")]
    [SerializeField] public int SquareWidth;
    [SerializeField] public int SquareHeight;

    [Header("Paint Settings")]
    [SerializeField] public List<HexPaintOption> HexPaintOptions;

    [Header("Transform Settings")]
    [SerializeField] public int TargetHeight;

    public Dictionary<Vector3Int, Hex> HexDictionary = new Dictionary<Vector3Int, Hex>();

    [Header("Generator Reference")]
    public HexGridGenerator HexGridGenerator;
    public HexPainter HexPainter;
    public HexBridgeGenerator HexBridgeGenerator;

    public bool IsEraseMode;

    private void OnEnable()
    {
        HexGridGenerator = gameObject.GetComponent<HexGridGenerator>();
        HexPainter = gameObject.GetComponent<HexPainter>();
        HexBridgeGenerator = gameObject.GetComponent<HexBridgeGenerator>();

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
                    Hex Hex = HexRay(Event, out Vector3 Hitpoint);
                    if(Hex) HexPainter.HandlePainting(Hex, IsEraseMode, HexPaintOptions);
                    Event.Use();
                }
                break;
            case EnumHexGridMode.Transform :
                if((Event.type == EventType.MouseDown || Event.type == EventType.MouseDrag) && Event.button == 0)
                {
                    switch (TransformTool)
                    {
                        case EnumTransformTool.SetHeight :
                            Hex Hex = HexRay(Event, out Vector3 Hitpoint);
                            if(Hex) Hex.SetHeight(TargetHeight);
                            Event.Use();                            
                            break;
                        default: break;
                    }
                }
                break;
            case EnumHexGridMode.Bridge :
                if(Ground && Event.type == EventType.MouseDown && Event.button == 0)
                {
                    Hex Hex = HexRay(Event, out Vector3 Hitpoint);
                    if(Hex)
                    {
                        if(!IsEraseMode)
                        {
                            HexBridgeGenerator.CreateHexNeighbor(Hex, Hitpoint, Ground.gameObject, TargetHeight, SquareWidth, SquareHeight);
                        }
                        else
                        {
                            HexBridgeGenerator.RemoveHex(Hex);
                        }
                    }
                    Event.Use();    
                }
                break;
            default: break;
        }
    }

    private Hex HexRay(Event Event, out Vector3 Hitpoint)
    {
        Hex Hex = null;
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if(!Physics.Linecast(ray.origin, hit.point)) 
            {
                Transform Parent = hit.collider.transform.parent;
                if (Parent != null && Parent.TryGetComponent<Hex>(out Hex CheckHex)) Hex = CheckHex;
            }
        }

        Hitpoint = hit.point;
        return Hex;
    }
}