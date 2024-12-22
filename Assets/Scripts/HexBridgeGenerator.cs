using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HexBridgeGenerator : BaseSignal
{
    private Transform HexParent;
    private List<Hex> AllHexes = new List<Hex>();

    private bool FindHexParent()
    {
        var PoolParent = GameObject.FindGameObjectWithTag("Pool");

        if (PoolParent == null) return false;

        HexParent = PoolParent.transform.Find("Hex")?.transform;

        return HexParent != null;
    }

    private void ConnectNeighborHexes(Hex Hex)
    {
        HashSet<Hex> Neighbors = new HashSet<Hex>();

        foreach (Vector3Int Direction in GetHexDirections())
        {
            Vector3Int NeighborCoords = Hex.Position + Direction;

            var NearHex = AllHexes.FirstOrDefault(X => X.Position == NeighborCoords);

            if (NearHex != null) Neighbors.Add(NearHex);
        }

        Hex.SetNeighborHexes(Neighbors);

        foreach (Hex Neighbor in Hex.ConnectedHexes)
        {
            HashSet<Hex> NeighborConnections = new HashSet<Hex>(Neighbor.ConnectedHexes) { Hex };
            Neighbor.SetNeighborHexes(NeighborConnections);
        }
    }

    private static List<Vector3Int> GetHexDirections() => new List<Vector3Int>
    {
        new Vector3Int(1, 0, 0),
        new Vector3Int(0, 0, 1),
        new Vector3Int(-1, 0, 1),
        new Vector3Int(-1, 0, 0),
        new Vector3Int(0, 0, -1),
        new Vector3Int(1, 0, -1)
    };

    public void CreateHexNeighbor(Hex HitHex, Vector3 HitPoint, GameObject HexPrefab, float TargetHeight, int GridWidth, int GridHeight)
    {
        if (!FindHexParent()) return;

        Vector3Int ClosestDirection = GetHexDirections()
            .OrderBy(Direction => Vector3.Distance(HitPoint, HexToPosition(HitHex.Position + Direction, GridWidth, GridHeight)))
            .First();

        Vector3Int NewCoords = HitHex.Position + ClosestDirection;
        Vector3 SpawnPosition = HexToPosition(NewCoords, GridWidth, GridHeight) + new Vector3(0, TargetHeight - 1, 0);

        GameObject NewHex = Instantiate(HexPrefab, SpawnPosition, Quaternion.identity, HexParent);

        Hex NewHexComponent = NewHex.GetComponent<Hex>();

        NewHexComponent.BridgeInit(new HashSet<object> { NewCoords });

        AllHexes = HexParent.GetComponentsInChildren<Hex>().ToList();
        AllHexes.Add(NewHexComponent);

        ConnectNeighborHexes(NewHexComponent);
    }

    public void RemoveHex(Hex Hex)
    {
        if(!Hex.IsBridge) return;

        foreach (Hex Neighbor in Hex.ConnectedHexes)
        {
            HashSet<Hex> UpdatedConnections = new HashSet<Hex>(Neighbor.ConnectedHexes);
            UpdatedConnections.Remove(Hex);
            Neighbor.SetNeighborHexes(UpdatedConnections);
        }

        AllHexes.Remove(Hex);
        DestroyImmediate(Hex.gameObject);
    }

    private Vector3 HexToPosition(Vector3Int Coords, int GridWidth, int GridHeight)
    {
        float X = Mathf.Sqrt(3) * Coords.x + Mathf.Sqrt(3) / 2 * Coords.z;
        float Z = 3.0f / 2 * Coords.z;

        Vector3 Offset = CalculateGridCenterOffset(GridWidth, GridHeight);
        return new Vector3(X, 0, Z) - Offset;
    }

    private Vector3 CalculateGridCenterOffset(int GridWidth, int GridHeight)
    {
        float Width = Mathf.Sqrt(3) * GridWidth;
        float Height = 1.5f * GridHeight;
        return new Vector3(Width / 2 - Mathf.Sqrt(3) / 2, 0, Height / 2 - 0.75f);
    }
}