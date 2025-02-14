using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class PathFinder : MonoBehaviour
{
    [Inject] Pool Pool;
    [Inject] EventBus EventBus;

    MoveComponent MoveComponent;

    void Start()
    {
        EventBus.Subscribe<PickHexSignal>(SignalBox);
        EventBus.Subscribe<ActionSignal>(SignalBox);
        EventBus.Subscribe<PickUnitSignal>(SignalBox);
    }

    void SignalBox<T>(T Obj)
    {
        switch (Obj)
        {
            case PickUnitSignal PickUnitSignal :
                MoveComponent = PickUnitSignal.Unit.GetComponentByType<MoveComponent>();
                ShowHidePickedHexes(Pool.GetAllOfType<Hex>(), false);
                break;

            case PickHexSignal PickHexSignal:

                if (MoveComponent == null) return;

                switch (MoveComponent.Master.State)
                {
                    case EnumUnitState.Stay :
                        MoveComponent?.SetNewPath(GenerateNearestWay(PickHexSignal.Hex.Position));
                        break;
                    case EnumUnitState.Jump :
                        var AllHexes = Pool.GetAllOfType<Hex>();
                        MoveComponent?.SetNewPath(new HashSet<Hex> {PickHexSignal.Hex, AllHexes.FirstOrDefault(x => x.Position == MoveComponent.Position)});
                        ShowHidePickedHexes(AllHexes, false);
                        break;
                    default: break;
                }
                break;

            case ActionSignal ActionSignal :
                switch (ActionSignal.EnumButtonSignals)
                {
                    case EnumButtonSignals.Walk :
                        if (ActionSignal.Unit.State != EnumUnitState.Stay) return;
                        GetHexesInRange(MoveComponent.CurrentTurnCount);
                        break;
                    case EnumButtonSignals.Jump :
                        if (ActionSignal.Unit.State != EnumUnitState.Stay) return;
                        if(GetHexesInRadiusWithJump(MoveComponent).Count != 0) MoveComponent.ReadyToJump();
                        break;
                    case EnumButtonSignals.Attack :
                        if (ActionSignal.Unit.State != EnumUnitState.Stay) return;

                        break;
                    default: break;
                }
                break;
                
            default: break;
        }
    }

    public void ShowHidePickedHexes(HashSet<Hex> Hexes, bool Switch)
    {
        if (Hexes.Count == 0) return;

        foreach (Hex Hex in Hexes) Hex.SetPickState(Switch);    
    }

    HashSet<Hex> GenerateNearestWay(Vector3Int FinalPoint)
    {
        HashSet<Hex> Hexes = new HashSet<Hex>(Pool.GetAllOfType<Hex>());
        if (Hexes.Count == 0) return null;

        ShowHidePickedHexes(Hexes, false);

        Vector3Int UnitPosition = MoveComponent.Position;
        HashSet<Hex> Path = FindShortestPath(UnitPosition, FinalPoint, Hexes);

        ShowHidePickedHexes(Path, true);

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
        Hex CenterHex = AllHexes.FirstOrDefault(x => x.Position == MoveComponent.Position);
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

        ShowHidePickedHexes(Result, true);

        return Result;
    }

    public HashSet<Hex> GetHexesInRadiusWithJump(MoveComponent MoveComponent)
    {
        var AllHexes = Pool.GetAllOfType<Hex>();

        ShowHidePickedHexes(AllHexes, false);

        Vector3 CenterWorldPos = MoveComponent.transform.position;
        Vector3Int CenterPos = MoveComponent.Position;
        int CenterHeight = CenterPos.y;

        HashSet<Hex> AllReachableHexes = new HashSet<Hex>();
        HashSet<Hex> Result = new HashSet<Hex>();

        var HexDictionary = AllHexes.GroupBy(H => new Vector3Int(H.Position.x, H.Position.y, H.Position.z))
            .ToDictionary(G => G.Key, G => G.First());

        Hex StartingHex = AllHexes.FirstOrDefault(H => H.Position == MoveComponent.Position);

        if (StartingHex == null)
        {
            Debug.LogError("Starting hex not found for the MoveComponent's position.");
            return Result;
        }

        for (int Radius = 1; Radius <= MoveComponent.JumpLength; Radius++)
        {
            List<Vector3Int> Ring = GetHexRing(CenterPos, Radius);

            foreach (var TargetPos in Ring)
            {
                foreach (var Hex in AllHexes.Where(H => H.Position.x == TargetPos.x && H.Position.z == TargetPos.z))
                {
                    AllReachableHexes.Add(Hex);
                }
            }
        }

        AllReachableHexes.RemoveWhere(Hex => !Hex.IsWalkable);


        HashSet<Hex> HexesAfterJumpCheck = new HashSet<Hex>();
        foreach (Hex Hex in AllReachableHexes)
        {
            if (MoveComponent.CanJumpToTarget(Hex.Enter.transform.position))
            {
                HexesAfterJumpCheck.Add(Hex);
            }
        }

        foreach (Hex TargetHex in HexesAfterJumpCheck)
        {
            int TotalCost = 0;
            bool IsPathBlocked = false;

            List<Vector3Int> Path = GetPathBetweenHexes(CenterPos, TargetHex.Position);

            foreach (var StepPos in Path)
            {
                Vector3Int StepKey = new Vector3Int(StepPos.x, StepPos.y, StepPos.z);

                if (!HexDictionary.TryGetValue(StepKey, out Hex CurrentHex))
                {
                    TotalCost += GetFlatOrDownCost();
                    continue;
                }

                int CurrentHeight = CurrentHex.Position.y;
                if (CurrentHeight > CenterHeight)
                {
                    int HeightDifference = CurrentHeight - CenterHeight;
                    TotalCost += HeightDifference * 1;
                }
                else continue;

                if (TotalCost > MoveComponent.JumpLength)
                {
                    IsPathBlocked = true;
                    break;
                }
            }

            if (!IsPathBlocked) Result.Add(TargetHex);
        }

        ShowHidePickedHexes(Result, true);

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

    void OnDestroy()
    {
        EventBus.Unsubscribe<PickHexSignal>(SignalBox);
        EventBus.Unsubscribe<ActionSignal>(SignalBox);
        EventBus.Unsubscribe<PickUnitSignal>(SignalBox);
    }
}