using System.Collections.Generic;
using UnityEngine;

public class HexGridGenerator : BaseSignal
{
    public Dictionary<Vector3Int, Hex> HexDictionary = new Dictionary<Vector3Int, Hex>();
    private GameObject HexParent;

    public void GenerateHexagonalGrid(int ArenaRadius, int TargetHeight, Hex Ground)
    {
        ClearHexGrid();
        FindOrCreateHexParent();

        for (int Q = -ArenaRadius; Q <= ArenaRadius; Q++)
        {
            for (int R = Mathf.Max(-ArenaRadius, -Q - ArenaRadius); R <= Mathf.Min(ArenaRadius, -Q + ArenaRadius); R++)
            {
                Vector3Int Coords = new Vector3Int(Q, TargetHeight, R);
                if (Ground == null)
                {
                    Debug.LogError("Ground prefab is not assigned.");
                    return;
                }

                Hex Hex = Instantiate(Ground, HexToPosition(Coords.x, Coords.z), Quaternion.identity, HexParent.transform);

                Hex.Initialization(new HashSet<object> { Coords });

                Hex.SetHeight(TargetHeight);

                Hex.SetLength(TargetHeight);

                HexDictionary[Coords] = Hex;
            }
        }

        ConnectNeighborHexes();
        HexParent.name = "Hex";
    }

    public void GenerateHexagonalGridByDimensions(int SquareHeight, int SquareWidth, int TargetHeight, Hex Ground)
    {
        ClearHexGrid();
        FindOrCreateHexParent();

        Vector3 centerOffset = CalculateGridCenterOffset(SquareHeight, SquareWidth);

        for (int R = 0; R < SquareHeight; R++)
        {
            for (int Q = 0; Q < SquareWidth; Q++)
            {
                int offset = R / 2;
                Vector3Int Coords = new Vector3Int(Q - offset, TargetHeight, R);
                if (Ground == null)
                {
                    Debug.LogError("Ground prefab is not assigned.");
                    return;
                }

                Hex Hex = Instantiate(Ground, HexToPosition(Coords.x, Coords.z) - centerOffset, Quaternion.identity, HexParent.transform);

                Hex.Initialization(new HashSet<object> { Coords });

                Hex.SetHeight(TargetHeight);

                Hex.SetLength(TargetHeight);

                HexDictionary[Coords] = Hex;
            }
        }

        ConnectNeighborHexes();
        HexParent.name = "Hex";
    }

    private void ConnectNeighborHexes()
    {
        foreach (var KVP in HexDictionary)
        {
            Vector3Int Coords = KVP.Key;
            Hex Hex = KVP.Value;

            HashSet<Hex> Neighbors = new HashSet<Hex>();

            foreach (Vector3Int Direction in GetHexDirections())
            {
                Vector3Int NeighborCoords = Coords + Direction;

                if (HexDictionary.TryGetValue(NeighborCoords, out Hex neighbor))
                {
                    Neighbors.Add(neighbor);
                }
            }

            Hex.SetNeighborHexes(Neighbors);
        }
    }

    private List<Vector3Int> GetHexDirections()
    {
        return new List<Vector3Int>
        {
            new Vector3Int(1, 0, 0),
            new Vector3Int(0, 0, 1),
            new Vector3Int(-1, 0, 1),
            new Vector3Int(-1, 0, 0),
            new Vector3Int(0, 0, -1),
            new Vector3Int(1, 0, -1)
        };
    }

    private Vector3 HexToPosition(int Q, int R)
    {
        float x = Mathf.Sqrt(3) * Q + Mathf.Sqrt(3) / 2 * R;
        float z = 3.0f / 2 * R;
        return new Vector3(x, 0, z);
    }

    private Vector3 CalculateGridCenterOffset(int SquareWidth, int SquareHeight)
    {
        float Width = Mathf.Sqrt(3) * SquareWidth;
        float Height = 1.5f * SquareHeight;
        return new Vector3(Width / 2 - 1 * Mathf.Sqrt(3) / 2, 0, Height / 2 - 0.75f);
    }

    public void ClearHexGrid()
    {
        GameObject PoolParent = GameObject.FindGameObjectWithTag("Pool");

        if (PoolParent)
        {
            foreach (Transform Child in PoolParent.transform)
            {
                if (Child.name.StartsWith("Hex"))
                {
                    DestroyImmediate(Child.gameObject);
                }
            }
        }

        HexDictionary.Clear();
        Debug.Log("Hex grid cleared.");
    }

    private void FindOrCreateHexParent()
    {
        var PoolParent = GameObject.FindGameObjectWithTag("Pool");

        if (!PoolParent)
        {
            PoolParent = new GameObject("Pool")
            {
                tag = "Pool"
            };
            PoolParent.transform.SetParent(GameObject.FindGameObjectWithTag("Systems")?.transform);
        }

        HexParent = PoolParent.transform.Find("Hex")?.gameObject;

        if (!HexParent)
        {
            HexParent = new GameObject("Hex");
            HexParent.transform.SetParent(PoolParent.transform);
        }
    }
}