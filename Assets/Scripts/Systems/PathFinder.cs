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

            case PickUnitSignal PickUnitSignal:
                TargetUnit = PickUnitSignal.Unit;
                if (TargetUnit.State != EnumUnitState.Stay) return;
                GetHexesInRange(TargetUnit.GetComponentByType<MoveComponent>().CurrentTurnCount);
                break;

            case JumpSignal JumpSignal:
                GetHexesInRadiusWithJump(JumpSignal.MoveComponent);
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

        Dictionary<Vector3Int, (int Cost, int MaxHeightSoFar)> CostMap = new Dictionary<Vector3Int, (int, int)>();
        CostMap[CenterPos] = (0, CenterHeight);

        Queue<Vector3Int> Queue = new Queue<Vector3Int>();
        Queue.Enqueue(CenterPos);

        var HexDictionary = AllHexes.GroupBy(h => new Vector2Int(h.Position.x, h.Position.z)).ToDictionary(g => g.Key, g => g.ToList());                  

        while (Queue.Count > 0)
        {
            Vector3Int CurrentPos = Queue.Dequeue();
            (int CurrentCost, int MaxHeightSoFar) = CostMap[CurrentPos];

            foreach (var Direction in HexLibrary.GetHexDirections())
            {
                Vector3Int NeighborPos = CurrentPos + Direction;
                Vector2Int Key = new Vector2Int(NeighborPos.x, NeighborPos.z);

                if (!HexDictionary.TryGetValue(Key, out List<Hex> MatchingHexes))
                {
                    int VirtualCost = CurrentCost + GetJumpOverCost();

                    if (VirtualCost > MoveComponent.JumpLength) continue;

                    if (CostMap.ContainsKey(NeighborPos) && CostMap[NeighborPos].Cost <= VirtualCost) continue;
                        
                    CostMap[NeighborPos] = (VirtualCost, MaxHeightSoFar);
                    Queue.Enqueue(NeighborPos);
                    continue;
                }

                foreach (var TargetHex in MatchingHexes)
                {
                    if (!TargetHex.IsWalkable || (TargetHex.Position.x == CenterHex.Position.x && TargetHex.Position.z == CenterHex.Position.z)) continue;

                    int TargetHeight = TargetHex.Position.y;

                    int NewMaxHeight = Mathf.Max(MaxHeightSoFar, TargetHeight);

                    int StepCost = ComputeStepCost(MaxHeightSoFar, TargetHeight);
                    int TotalCost = CurrentCost + StepCost;

                    if (TotalCost > MoveComponent.JumpLength) continue;

                    if (CostMap.ContainsKey(TargetHex.Position) && CostMap[TargetHex.Position].Cost <= TotalCost) continue;
            
                    CostMap[TargetHex.Position] = (TotalCost, NewMaxHeight);

                    Result.Add(TargetHex);
                    Queue.Enqueue(TargetHex.Position);
                }
            }
        }

        foreach (Hex Hex in Result) Hex.SetPickState(true);

        return Result;
    }

    private int GetJumpOverCost() => 1;

    private int ComputeStepCost(int MaxHeightSoFar, int TargetHeight)
    {
        if (TargetHeight <= MaxHeightSoFar)
        {
            return GetFlatOrDownCost();
        }
        else
        {
            return ComputeJumpCost(MaxHeightSoFar, TargetHeight);
        }
    }

    private int ComputeJumpCost(int CurrentHeight, int TargetHeight) => TargetHeight - CurrentHeight;

    private int GetFlatOrDownCost() => 1;
}