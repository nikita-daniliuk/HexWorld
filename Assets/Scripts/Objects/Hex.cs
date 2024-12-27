using UnityEngine;
using System.Collections.Generic;
using Zenject;

public class Hex : Unit
{
    [Inject] EventBus EventBus;

    [SerializeField] private Vector3Int _Position;
    public Vector3Int Position => _Position;
    
    [SerializeField] private bool _IsWalkable = true;
    public bool IsWalkable => _IsWalkable;

    [SerializeField] private int _Lenght = 1;
    public int Lenght => _Lenght;

    [SerializeField] private List<Hex> ConnectedHexesList;
    public IReadOnlyList<Hex> ConnectedHexes => ConnectedHexesList;

    public GameObject Pick;
    public GameObject Enter;
    public MeshRenderer HexVisual;
    public GameObject Walkable;
    public GameObject NotWalkable;

    public override void Initialization(HashSet<object> Systems)
    {
        this.Systems.UnionWith(Systems);
        _Position = GetSystemByType<Vector3Int>();
    }

    public void SetHeight(int Height)
    {
        _Position = new Vector3Int(Position.x, Mathf.Max(1, Height), Position.z);
        transform.localScale = new Vector3(1, Height, 1);
        transform.position = new Vector3(transform.position.x, Height - 1, transform.position.z);

        UpdateEmblemPosition();
    }

    public void UpdateEmblemPosition()
    {
        Pick.transform.position = new Vector3(Pick.transform.position.x, Position.y - 1 + 0.1f, Pick.transform.position.z);
        Enter.transform.position = new Vector3(Enter.transform.position.x, Position.y - 1 + 0.05f, Enter.transform.position.z);

        Walkable.transform.position = new Vector3(Walkable.transform.position.x, Position.y - 1 + 0.1f, Walkable.transform.position.z);
        NotWalkable.transform.position = new Vector3(NotWalkable.transform.position.x, Position.y - 1 + 0.1f, NotWalkable.transform.position.z);      
    }

    public void SetNeighborHexes(HashSet<Hex> ConnectedHexes)
    {
        ConnectedHexesList = new List<Hex>(ConnectedHexes);
    }

    public void SetPickState(bool IsActive)
    {
        Pick.SetActive(IsActive);
    }

    public void SetIsWalkable(bool Value)
    {
        _IsWalkable = Value;
    }

    public void SetLength(int Lenght)
    {
        _Lenght = Lenght;
        transform.localScale = new Vector3(1, Lenght, 1);
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