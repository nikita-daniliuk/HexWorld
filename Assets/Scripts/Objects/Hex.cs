using UnityEngine;
using System.Collections.Generic;
using Zenject;

public class Hex : Unit
{
    [Inject] EventBus EventBus;

    [SerializeField] private Vector3Int _Position;
    public Vector3Int Position => _Position;
    
    [SerializeField] private bool _IsBisy;
    public bool IsBisy => _IsBisy;

    [SerializeField] private List<Hex> ConnectedHexesList;
    public IReadOnlyList<Hex> ConnectedHexes => ConnectedHexesList;

    [Header("Emblems")]
    public GameObject Pick;
    public GameObject Enter;
    public MeshRenderer HexVisual;

    [SerializeField] private bool _IsBridge;
    public bool IsBridge => _IsBridge;

    public override void Initialization(HashSet<object> Systems)
    {
        this.Systems.UnionWith(Systems);
        _Position = GetSystemByType<Vector3Int>();
        SetHeight(Position.y);
    }

    public void BridgeInit(HashSet<object> Systems)
    {
        this.Systems.UnionWith(Systems);
        _Position = GetSystemByType<Vector3Int>();  
        _IsBridge = true;  
    }

    public void SetHeight(int Height)
    {
        if(_IsBridge) return;
        
        _Position = new Vector3Int(Position.x, Mathf.Max(1, Height), Position.z);
        HexVisual.transform.localScale = new Vector3(1, Height, 1);

        Pick.transform.position = new Vector3(Pick.transform.position.x, Position.y - 1 + 0.1f, Pick.transform.position.z);
        Enter.transform.position = new Vector3(Enter.transform.position.x, Position.y - 1 + 0.05f, Enter.transform.position.z);
    }

    public void SetNeighborHexes(HashSet<Hex> ConnectedHexes)
    {
        ConnectedHexesList = new List<Hex>(ConnectedHexes);
    }

    public void SetPickState(bool IsActive)
    {
        Pick.SetActive(IsActive);
    }

    public void SetBisyState(bool Value)
    {
        _IsBisy = Value;
    }

    public void SetHexVisual(MeshRenderer NewHexVisual)
    {
        var NewHex = Instantiate(NewHexVisual, transform);
        NewHex.transform.position = HexVisual.transform.position;
        NewHex.transform.rotation = HexVisual.transform.rotation;
        NewHex.transform.localScale = HexVisual.transform.localScale;

        DestroyImmediate(HexVisual.gameObject);

        HexVisual = NewHex;
    }

    private void OnMouseDown()
    {
        if (!Pick.activeSelf) return;
        EventBus.Invoke(new PickHexSignal(this));
    }

    private void OnMouseEnter()
    {
        Enter.SetActive(HexVisual.enabled);
    }

    private void OnMouseExit()
    {
        Enter.SetActive(false);
    }
}