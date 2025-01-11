using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class PathFinder : MonoBehaviour
{
    [Inject] Pool Pool;
    [Inject] EventBus EventBus;

    Unit TargetUnit;

    void Start() => EventBus.Subscribe(SignalBox);

    void SignalBox(object Obj)
    {
        switch (Obj)
        {
            case PickHexSignal PickHexSignal:

                if (TargetUnit == null) return;

                switch (TargetUnit.State)
                {
                    case EnumUnitState.Stay :
                        EventBus.Invoke(new PathSignal(GenerateNearestWay(PickHexSignal.Hex.Position)));
                        break;
                    case EnumUnitState.Jump :
                        EventBus.Invoke(new PathSignal(new HashSet<Hex> {PickHexSignal.Hex}));
                        HideAllPickedHexes();
                        break;
                    default: break;
                }
                break;

            case UnitWalkSignal UnitWalkSignal:
                TargetUnit = UnitWalkSignal.Unit;
                if (TargetUnit.State != EnumUnitState.Stay) return;
                GetHexesInRange(TargetUnit.GetComponentByType<MoveComponent>().CurrentTurnCount);
                break;

            case JumpSignal JumpSignal:
                GetHexesInRadiusWithJump(JumpSignal.MoveComponent);
                break;

            case PickUnitSignal PickUnitSignal :
                TargetUnit = PickUnitSignal.Unit;
                break;
                
            default: break;
        }
    }

    void HideAllPickedHexes()
    {
        HashSet<Hex> Hexes = new HashSet<Hex>(Pool.GetAllOfType<Hex>());
        if (Hexes.Count == 0) return;

        foreach (Hex Hex in Hexes) Hex.SetPickState(false);      
    }

    HashSet<Hex> GenerateNearestWay(Vector3Int FinalPoint)
    {
        HashSet<Hex> Hexes = new HashSet<Hex>(Pool.GetAllOfType<Hex>());
        if (Hexes.Count == 0) return null;

        foreach (Hex Hex in Hexes) Hex.SetPickState(false);

        Vector3Int UnitPosition = TargetUnit.GetComponentByType<MoveComponent>().Position;
        HashSet<Hex> Path = FindShortestPath(UnitPosition, FinalPoint, Hexes);

        Path.RemoveWhere(X => X.Position == UnitPosition);

        foreach (Hex Hex in Path) Hex.SetPickState(true);

        return Path;
    }

    HashSet<Hex> FindShortestPath(Vector3Int Start, Vector3Int End, HashSet<Hex> Hexes)
    {
        if (Hexes.Count == 0) return new HashSet<Hex>();

        Queue<(Hex Hex, int CurrentHeight)> Queue = new Queue<(Hex, int)>();

        Dictionary<Hex, Hex> Parent = new Dictionary<Hex, Hex>();

        HashSet<Hex> Visited = new HashSet<Hex>();

        Hex StartHex = Hexes.FirstOrDefault(x => x.Position == Start);
        Hex EndHex = Hexes.FirstOrDefault(x => x.Position == End);
        if (StartHex == null || EndHex == null) return new HashSet<Hex>();

        Queue.Enqueue((StartHex, StartHex.Position.y));
        Visited.Add(StartHex);
        Parent[StartHex] = null;

        while (Queue.Count > 0)
        {
            var (Current, CurrentHeight) = Queue.Dequeue();

            if (Current == EndHex)
            {
                List<Hex> Path = new List<Hex>();
                for (Hex Step = EndHex; Step != null; Step = Parent[Step]) Path.Add(Step);

                Path.Reverse();
                return new HashSet<Hex>(Path);
            }

            foreach (Hex Neighbor in Current.ConnectedHexes)
            {
                if (Visited.Contains(Neighbor) || !Neighbor.IsWalkable) continue;

                int HeightDiff = Neighbor.Position.y - CurrentHeight;

                if (Mathf.Abs(HeightDiff) > 1) continue;

                Visited.Add(Neighbor);
                Queue.Enqueue((Neighbor, Neighbor.Position.y));
                Parent[Neighbor] = Current;
            }
        }

        return new HashSet<Hex>();
    }

    public HashSet<Hex> GetHexesInRange(int Range)
    {
        HashSet<Hex> AllHexes = new HashSet<Hex>(Pool.GetAllOfType<Hex>());
        foreach (Hex Hex in AllHexes) Hex.SetPickState(false);

        HashSet<Hex> Result = new HashSet<Hex>();
        Hex CenterHex = AllHexes.FirstOrDefault(x => x.Position == TargetUnit.GetComponentByType<MoveComponent>().Position);
        if (CenterHex == null) return Result;

        Queue<(Hex Hex, int Dist, int MinHeight, int MaxHeight)> Queue = new Queue<(Hex, int, int, int)>();
        HashSet<Hex> Visited = new HashSet<Hex>();

        Queue.Enqueue((CenterHex, 0, CenterHex.Position.y, CenterHex.Position.y));
        Visited.Add(CenterHex);

        while (Queue.Count > 0)
        {
            var (CurrentHex, CurrentDist, MinHeightSoFar, MaxHeightSoFar) = Queue.Dequeue();
            if (CurrentDist >= Range) continue;

            foreach (Hex Neighbor in CurrentHex.ConnectedHexes)
            {
                if (Visited.Contains(Neighbor) || !Neighbor.IsWalkable) continue;

                int NeighborHeight = Neighbor.Position.y;
                if (NeighborHeight > MaxHeightSoFar + 1 || NeighborHeight < MinHeightSoFar - 1) continue;

                int NewMinHeight = Mathf.Min(MinHeightSoFar, NeighborHeight);
                int NewMaxHeight = Mathf.Max(MaxHeightSoFar, NeighborHeight);

                Visited.Add(Neighbor);
                int NextDist = CurrentDist + 1;
                Queue.Enqueue((Neighbor, NextDist, NewMinHeight, NewMaxHeight));

                if (NextDist <= Range) Result.Add(Neighbor);
            }
        }

        foreach (Hex Hex in Result) Hex.SetPickState(true);

        return Result;
    }

    public HashSet<Hex> GetHexesInRadiusWithJump(MoveComponent MoveComponent)
    {
        var AllHexes = Pool.GetAllOfType<Hex>();

        foreach (Hex Hex in AllHexes) Hex.SetPickState(false);

        Hex CenterHex = AllHexes.FirstOrDefault(h => h.Position == MoveComponent.Position);
        if (CenterHex == null)
        {
            Debug.LogError("CenterHex is null!");
            return new HashSet<Hex>();
        }

        Vector3Int CenterPos = CenterHex.Position;
        int CenterHeight = CenterHex.Position.y;

        HashSet<Hex> Result = new HashSet<Hex>();

        var HexDictionary = AllHexes.GroupBy(h => new Vector2Int(h.Position.x, h.Position.z))
            .ToDictionary(g => g.Key, g => g.First());

        for (int radius = 1; radius <= MoveComponent.JumpLength; radius++)
        {
            List<Vector3Int> Ring = GetHexRing(CenterPos, radius);

            foreach (var TargetPos in Ring)
            {
                if (!HexDictionary.TryGetValue(new Vector2Int(TargetPos.x, TargetPos.z), out Hex TargetHex)) continue;

                if (!TargetHex.IsWalkable) continue;

                int TotalCost = 0;

                List<Vector3Int> Path = GetPathBetweenHexes(CenterPos, TargetPos);

                bool IsPathBlocked = false;

                foreach (var StepPos in Path)
                {
                    Vector2Int Key = new Vector2Int(StepPos.x, StepPos.z);

                    if (!HexDictionary.TryGetValue(Key, out Hex CurrentHex))
                    {
                        TotalCost += GetFlatOrDownCost();
                        continue;
                    }

                    int CurrentHeight = CurrentHex.Position.y;

                    if (CurrentHeight > CenterHeight)
                    {
                        int HeightDifference = CurrentHeight - CenterHeight;
                        TotalCost += HeightDifference;
                    }
                    else
                    {
                        TotalCost += GetFlatOrDownCost();
                    }

                    if (TotalCost > MoveComponent.JumpLength)
                    {
                        IsPathBlocked = true;
                        break;
                    }
                }

                if (IsPathBlocked) continue;

                Result.Add(TargetHex);
            }
        }

        foreach (Hex Hex in Result) Hex.SetPickState(true);

        return Result;
    }

    private List<Vector3Int> GetHexRing(Vector3Int Center, int Radius)
    {
        List<Vector3Int> Ring = new List<Vector3Int>();

        Vector3Int Current = Center + HexLibrary.GetHexDirections()[4] * Radius;

        foreach (var Direction in HexLibrary.GetHexDirections())
        {
            for (int i = 0; i < Radius; i++)
            {
                Ring.Add(Current);
                Current += Direction;
            }
        }

        return Ring;
    }

    private List<Vector3Int> GetPathBetweenHexes(Vector3Int Start, Vector3Int End)
    {
        List<Vector3Int> Path = new List<Vector3Int>();

        int N = Mathf.Max(Mathf.Abs(Start.x - End.x), Mathf.Abs(Start.y - End.y), Mathf.Abs(Start.z - End.z));
        for (int i = 1; i <= N; i++)
        {
            float t = i / (float)N;
            int x = Mathf.RoundToInt(Mathf.Lerp(Start.x, End.x, t));
            int y = Mathf.RoundToInt(Mathf.Lerp(Start.y, End.y, t));
            int z = Mathf.RoundToInt(Mathf.Lerp(Start.z, End.z, t));
            Path.Add(new Vector3Int(x, y, z));
        }

        return Path;
    }

    private int GetFlatOrDownCost() => 1;
}