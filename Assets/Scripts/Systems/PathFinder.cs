using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class PathFinder : MonoBehaviour
{
    [Inject] Pool Pool;
    [Inject] EventBus EventBus;
    Player PickedUnit;

    void Start()
    {
        EventBus.Subscribe(SignalBox);
    }

    void SignalBox(object Obj)
    {
        switch (Obj)
        {
            case PickHexSignal PickHexSignal:
                if (PickedUnit == null || PickedUnit.State != EnumUnitState.Stay) return;
                GenerateNearestWay(PickHexSignal.Hex.Position);
                break;
            case PickUnitSignal PickUnitSignal:
                PickedUnit = PickUnitSignal.Unit as Player;
                if(PickedUnit.State != EnumUnitState.Stay) return;
                GetHexesInRange(PickedUnit.GetComponentByType<MoveComponent>().CurrentTurnCount);
                break;
            default: break;
        }
    }

    void GenerateNearestWay(Vector3Int FinalPoint)
    {
        HashSet<Hex> Hexes = new HashSet<Hex>(Pool.GetAllOfType<Hex>());

        if (Hexes.Count == 0) return;

        foreach (Hex Hex in Hexes)
        {
            Hex.SetPickState(false);
        }   

        var UnitPosition = PickedUnit.GetComponentByType<MoveComponent>().Position;

        HashSet<Hex> Path = FindShortestPath(UnitPosition, FinalPoint, Hexes);

        Path.RemoveWhere(x => x.Position == UnitPosition);

        EventBus.Invoke(new PathSignal(Path));

        foreach (Hex Hex in Path)
        {
            Hex.SetPickState(true);
        }
    }

    HashSet<Hex> FindShortestPath(Vector3Int Start, Vector3Int End, HashSet<Hex> Hexes)
    {
        if (Hexes.Count == 0) return new HashSet<Hex>();

        Queue<Hex> Queue = new Queue<Hex>();
        Dictionary<Hex, Hex> Parent = new Dictionary<Hex, Hex>();
        HashSet<Hex> Visited = new HashSet<Hex>();

        Hex StartHex = Hexes.FirstOrDefault(x => x.Position == Start);
        Hex EndHex = Hexes.FirstOrDefault(x => x.Position == End);

        if (StartHex == null || EndHex == null)
        {
            Debug.LogError("Start or end hex is null.");
            return new HashSet<Hex>();
        }

        Queue.Enqueue(StartHex);
        Visited.Add(StartHex);
        Parent[StartHex] = null;

        while (Queue.Count > 0)
        {
            Hex Current = Queue.Dequeue();

            if (Current == EndHex)
            {
                List<Hex> Path = new List<Hex>();
                for (Hex Step = EndHex; Step != null; Step = Parent[Step])
                {
                    Path.Add(Step);
                }

                Path.Reverse();

                return new HashSet<Hex>(Path);
            }

            foreach (Hex Neighbor in Current.ConnectedHexes)
            {
                int HeightDiff = Mathf.Abs(Neighbor.Position.y - Current.Position.y);
                if (!Visited.Contains(Neighbor) && !Neighbor.IsBisy && HeightDiff <= 1)
                {
                    Visited.Add(Neighbor);
                    Queue.Enqueue(Neighbor);
                    Parent[Neighbor] = Current;
                }
            }
        }
        return new HashSet<Hex>();
    }

    public HashSet<Hex> GetHexesInRange(int Range)
    {
        HashSet<Hex> AllHexes = new HashSet<Hex>(Pool.GetAllOfType<Hex>());

        foreach (Hex Hex in AllHexes)
        {
            Hex.SetPickState(false);
        }

        HashSet<Hex> HexesInRange = new HashSet<Hex>();

        Hex CenterHex = AllHexes.FirstOrDefault(x => x.Position == PickedUnit.GetComponentByType<MoveComponent>().Position);
        if (CenterHex == null) return HexesInRange;

        HashSet<Hex> Visited = new HashSet<Hex>();
        Queue<(Hex, int)> Queue = new Queue<(Hex, int)>();

        Queue.Enqueue((CenterHex, 0));
        Visited.Add(CenterHex);

        while (Queue.Count > 0)
        {
            var (CurrentHex, CurrentDistance) = Queue.Dequeue();

            if (CurrentDistance >= Range) continue;

            foreach (Hex Neighbor in CurrentHex.ConnectedHexes)
            {
                int HeightDiff = Mathf.Abs(Neighbor.Position.y - CurrentHex.Position.y);
                if (Visited.Contains(Neighbor) || Neighbor.IsBisy || HeightDiff > 1) continue;

                Visited.Add(Neighbor);
                Queue.Enqueue((Neighbor, CurrentDistance + 1));

                if (CurrentDistance + 1 <= Range)
                {
                    HexesInRange.Add(Neighbor);
                }
            }
        }

        foreach (Hex Hex in HexesInRange)
        {
            Hex.SetPickState(true);
        }

        return HexesInRange;
    }
}