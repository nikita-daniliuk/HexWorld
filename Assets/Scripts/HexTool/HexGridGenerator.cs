using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using System.Collections.Generic;

public class HexGridGenerator : BaseSignal
{
    public Dictionary<Vector3Int, Hex> HexDictionary = new Dictionary<Vector3Int, Hex>();
    private GameObject HexParent;
    public float GenerationTime { get; private set; }

    public void GenerateHexagonalGridWithJobs(int ArenaRadius, int TargetHeight, Hex Ground, List<HexPaintOption> HexPaintOptions)
    {
        ClearHexGrid();
        FindOrCreateHexParent();

        List<Vector3Int> ValidCoords = GenerateHexCoordinates(ArenaRadius);
        GenerateGridWithJobs(ValidCoords, TargetHeight, Ground, HexPaintOptions);
    }

    public void GenerateSquareGridWithJobs(int SquareWidth, int SquareHeight, int TargetHeight, Hex Ground, List<HexPaintOption> HexPaintOptions)
    {
        ClearHexGrid();
        FindOrCreateHexParent();

        List<Vector3Int> ValidCoords = GenerateSquareCoordinates(SquareWidth, SquareHeight);
        GenerateGridWithJobs(ValidCoords, TargetHeight, Ground, HexPaintOptions);
    }

    private void GenerateGridWithJobs(List<Vector3Int> ValidCoords, int TargetHeight, Hex Ground, List<HexPaintOption> HexPaintOptions)
    {
        float StartTime = Time.realtimeSinceStartup;

        int TotalHexes = ValidCoords.Count;

        NativeArray<Vector3Int> CoordsArray = new NativeArray<Vector3Int>(TotalHexes, Allocator.TempJob);
        NativeArray<Vector3> PositionsArray = new NativeArray<Vector3>(TotalHexes, Allocator.TempJob);

        for (int i = 0; i < TotalHexes; i++)
        {
            CoordsArray[i] = ValidCoords[i];
        }

        HexGridJob hexGridJob = new HexGridJob
        {
            Coords = CoordsArray,
            Positions = PositionsArray
        };

        JobHandle jobHandle = hexGridJob.Schedule(TotalHexes, 64);
        jobHandle.Complete();

        Vector3 CenterOfMass = Vector3.zero;
        for (int i = 0; i < TotalHexes; i++)
        {
            CenterOfMass += PositionsArray[i];
        }
        CenterOfMass /= TotalHexes;

        for (int i = 0; i < TotalHexes; i++)
        {
            Vector3Int Coords = CoordsArray[i];
            Vector3 Position = PositionsArray[i] - CenterOfMass;

            Hex Hex = Instantiate(Ground, Position, Quaternion.identity, HexParent.transform);
            Hex.Initialization(new HashSet<object> { Coords });

            MeshRenderer SelectedHex = null;
            foreach (var Option in HexPaintOptions)
            {
                if (Option.IsSelected && Option.HexPrefab != null)
                {
                    SelectedHex = Option.HexPrefab;
                    break;
                }
            }

            Hex.SetHexVisual(SelectedHex);
            Hex.SetHeight(TargetHeight);
            Hex.SetLength(TargetHeight);

            HexDictionary[Coords] = Hex;
        }

        CoordsArray.Dispose();
        PositionsArray.Dispose();

        ConnectNeighborHexes();
        HexParent.name = "Hex";

        GenerationTime = Time.realtimeSinceStartup - StartTime;
        Debug.Log($"Grid generation completed in {GenerationTime:F2} seconds.");
    }

    private List<Vector3Int> GenerateHexCoordinates(int Radius)
    {
        List<Vector3Int> Coords = new List<Vector3Int>();

        for (int Q = -Radius; Q <= Radius; Q++)
        {
            for (int R = Mathf.Max(-Radius, -Q - Radius); R <= Mathf.Min(Radius, -Q + Radius); R++)
            {
                Coords.Add(new Vector3Int(Q, 0, R));
            }
        }

        return Coords;
    }

    private List<Vector3Int> GenerateSquareCoordinates(int Width, int Height)
    {
        List<Vector3Int> Coords = new List<Vector3Int>();

        Vector3 CenterOffset = CalculateGridCenterOffset(Height, Width);

        for (int R = 0; R < Height; R++)
        {
            int Offset = R / 2;

            for (int Q = 0; Q < Width; Q++)
            {
                Vector3Int CoordsWithoutCentering = new Vector3Int(Q - Offset, 0, R);
                Vector3Int CenteredCoords = CoordsWithoutCentering - Vector3Int.RoundToInt(CenterOffset);
                Coords.Add(CenteredCoords);
            }
        }

        return Coords;
    }

    private Vector3 CalculateGridCenterOffset(int SquareHeight, int SquareWidth)
    {
        float Width = Mathf.Sqrt(3) * (SquareWidth - 1);
        float Height = 1.5f * (SquareHeight - 1);
        return new Vector3(Width / 2f, 0, Height / 2f);
    }

    private void ConnectNeighborHexes()
    {
        foreach (var KVP in HexDictionary)
        {
            Vector3Int Coords = KVP.Key;
            Hex Hex = KVP.Value;

            HashSet<Hex> Neighbors = new HashSet<Hex>();

            foreach (Vector3Int Direction in HexLibrary.GetHexDirections())
            {
                Vector3Int NeighborCoords = Coords + Direction;

                if (HexDictionary.TryGetValue(NeighborCoords, out Hex Neighbor))
                {
                    Neighbors.Add(Neighbor);
                }
            }

            Hex.SetNeighborHexes(Neighbors);
        }
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

    public void ResetProgress() => GenerationTime = 0f;
}

public struct HexGridJob : IJobParallelFor
{
    [ReadOnly]
    public NativeArray<Vector3Int> Coords;

    [WriteOnly]
    public NativeArray<Vector3> Positions;

    public void Execute(int Index)
    {
        Vector3Int Coords = this.Coords[Index];

        float x = Mathf.Sqrt(3) * Coords.x + Mathf.Sqrt(3) / 2 * Coords.z;
        float z = 3.0f / 2 * Coords.z;
        Positions[Index] = new Vector3(x, 0, z);
    }
}