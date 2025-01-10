using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HexCreator : BaseSignal
{
    private Transform HexParent;
    private List<Hex> AllHexes = new List<Hex>();

    public void Refresh() => AllHexes.Clear();

    private bool FindHexParent()
    {
        var poolParent = GameObject.FindGameObjectWithTag("Pool");
        if (poolParent == null) return false;
        HexParent = poolParent.transform.Find("Hex")?.transform;
        return HexParent != null;
    }

    private void ConnectNeighborHexes(Hex hex)
    {
        HashSet<Hex> neighbors = new HashSet<Hex>();
        foreach (Vector3Int direction in HexLibrary.GetHexDirections())
        {
            Vector3Int neighborCoords = hex.Position + direction;
            var nearHexes = AllHexes
                .FindAll(x => x.Position.x == neighborCoords.x && x.Position.z == neighborCoords.z);

            if (nearHexes != null)
                neighbors.UnionWith(nearHexes);
        }

        hex.SetNeighborHexes(neighbors);

        foreach (Hex neighbor in hex.ConnectedHexes)
        {
            HashSet<Hex> neighborConnections = new HashSet<Hex>(neighbor.ConnectedHexes) { hex };
            neighbor.SetNeighborHexes(neighborConnections);
        }
    }

    public GameObject CreateHexNeighbor(
        Hex hitHex,
        RaycastHit hit,
        GameObject hexPrefab,
        List<HexPaintOption> HexPaintOptions,
        int targetHeight,
        int targetLength,
        int gridWidth,
        int gridHeight
    )
    {
        if (!FindHexParent()) return null;

        float angle = Vector3.Angle(hit.normal, Vector3.up);
        bool clickedOnTop = angle < 30f;

        Vector3Int newCoords;

        if (clickedOnTop)
        {
            int newTop = targetHeight;

            newCoords = new Vector3Int(hitHex.Position.x, newTop, hitHex.Position.z);

            if (!IsValidHexPosition(newCoords, newTop, targetLength))
                return null;

            targetHeight = newTop;
        }
        else
        {
            Vector3Int closestDirection = HexLibrary.GetHexDirections()
                .OrderBy(d =>
                    Vector3.Distance(
                        hit.point,
                        HexToPosition(hitHex.Position + d, gridWidth, gridHeight)
                    )
                )
                .First();

            newCoords = hitHex.Position + closestDirection;

            if (!IsValidHexPosition(newCoords, targetHeight, targetLength))
                return null;
        }

        Vector3 spawnPosition = HexToPosition(newCoords, gridWidth, gridHeight);

        GameObject newHex = Instantiate(hexPrefab, HexParent.transform);
        newHex.transform.position = spawnPosition;

        MeshRenderer SelectedHex = null;
        foreach (var Option in HexPaintOptions)
        {
            if (Option.IsSelected && Option.HexPrefab != null)
            {
                SelectedHex = Option.HexPrefab;
                break;
            }
        }

        Hex newHexComponent = newHex.GetComponent<Hex>();
        newHexComponent.Initialization(new HashSet<object> { newCoords });
        newHexComponent.SetHeight(targetHeight);
        newHexComponent.SetLength(targetLength);
        newHexComponent.SetHexVisual(SelectedHex);
        hitHex.SetIsWalkable(!(hitHex.Position.y == newHexComponent.Position.y - newHexComponent.Lenght));
        newHexComponent.UpdateEmblemPosition();

        AllHexes = HexParent.GetComponentsInChildren<Hex>().ToList();
        AllHexes.Add(newHexComponent);

        ConnectNeighborHexes(newHexComponent);

        return newHex;
    }

    private bool IsValidHexPosition(Vector3Int coords, int targetHeight, int targetLength)
    {
        int newStart = targetHeight - targetLength + 1;
        int newEnd   = targetHeight;

        if(AllHexes.Count == 0) AllHexes = HexParent.GetComponentsInChildren<Hex>().ToList();

        var sameXZHexes = AllHexes
            .Where(h => h.Position.x == coords.x && h.Position.z == coords.z)
            .OrderBy(h => h.Position.y)
            .ToList();

        if (sameXZHexes.Count == 0) return true;

        var occupiedRanges = new List<(int Start, int End)>();
        foreach (var neighbor in sameXZHexes)
        {
            int start = neighbor.Position.y - neighbor.Lenght + 1;
            int end   = neighbor.Position.y;
            occupiedRanges.Add((start, end));
        }

        occupiedRanges = occupiedRanges.OrderBy(r => r.Start).ToList();
        var mergedRanges = new List<(int Start, int End)>();

        foreach (var range in occupiedRanges)
        {
            if (mergedRanges.Count == 0)
            {
                mergedRanges.Add(range);
            }
            else
            {
                var last = mergedRanges[mergedRanges.Count - 1];
                if (range.Start <= last.End + 1)
                {
                    int mergedEnd = Mathf.Max(last.End, range.End);
                    mergedRanges[mergedRanges.Count - 1] = (last.Start, mergedEnd);
                }
                else
                {
                    mergedRanges.Add(range);
                }
            }
        }

        var freeRanges = new List<(int Start, int End)>();
        int pointer = int.MinValue;

        foreach (var (start, end) in mergedRanges)
        {
            if (start > pointer)
            {
                freeRanges.Add((pointer, start - 1));
            }
            pointer = Mathf.Max(pointer, end + 1);
        }

        freeRanges.Add((pointer, int.MaxValue));

        foreach (var (freeStart, freeEnd) in freeRanges)
        {
            if (newStart >= freeStart && newEnd <= freeEnd)
                return true;
        }
        return false;
    }

    public void RemoveHex(Hex hex)
    {
        foreach (Hex neighbor in hex.ConnectedHexes)
        {
            var updatedConnections = new HashSet<Hex>(neighbor.ConnectedHexes);
            updatedConnections.Remove(hex);
            neighbor.SetNeighborHexes(updatedConnections);
        }
        AllHexes.Remove(hex);
        DestroyImmediate(hex.gameObject);
    }

    private Vector3 HexToPosition(Vector3Int coords, int gridWidth, int gridHeight)
    {
        float x = Mathf.Sqrt(3) * coords.x + (Mathf.Sqrt(3) / 2f) * coords.z;
        float z = 1.5f * coords.z;

        Vector3 offset = CalculateGridCenterOffset(gridWidth, gridHeight);
        return new Vector3(x, 0, z) - offset;
    }

    private Vector3 CalculateGridCenterOffset(int gridWidth, int gridHeight)
    {
        float width  = Mathf.Sqrt(3) * gridWidth;
        float height = 1.5f * gridHeight;
        return new Vector3(
            width  / 2f - (Mathf.Sqrt(3) / 2f),
            0,
            height / 2f - 0.75f
        );
    }
}