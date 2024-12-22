using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MoveComponent : Components, IFixedUpdate
{
    EventBus EventBus;
    WorldUpdateSystem WorldUpdateSystem;
    StepByStepSystem StepByStepSystem;

    [SerializeField] float MoveSpeed;
    [SerializeField] int TurnCount;
    public int CurrentTurnCount { get; private set; }
    public Vector3Int Position;

    private HashSet<Hex> AvailableHexes = new HashSet<Hex>();

    public override void Initialization(Unit Master)
    {
        CurrentTurnCount = TurnCount;
        ExtractSystems(Master.Systems);
    }

    protected override void ExtractSystems(HashSet<object> Systems)
    {
        foreach (var System in Systems)
        {
            switch (System)
            {
                case EventBus EventBus:
                    this.EventBus = EventBus;
                    EventBus.Subscribe(SignalBox);
                    break;
                case WorldUpdateSystem WorldUpdateSystem:
                    this.WorldUpdateSystem = WorldUpdateSystem;
                    break;
                case StepByStepSystem StepByStepSystem:
                    this.StepByStepSystem = StepByStepSystem;
                    break;
                default:
                    break;
            }
        }
    }

    protected override void SignalBox(object Obj)
    {
        switch (Obj)
        {
            case PathSignal PathSignal:
                EmitSignal(EnumMoveSignals.StartMoving);
                AvailableHexes.Clear();
                AvailableHexes = PathSignal.Hexes.ToHashSet();
                WorldUpdateSystem.Subscribe(this);
                break;

            case EnumSignals.NextTurn:
                CurrentTurnCount = TurnCount;
                break;

            default: break;
        }
    }

    public void FixedRefresh() => Move();

    private void Move()
    {
        Hex TargetHex = AvailableHexes.FirstOrDefault();

        if (TargetHex == null)
            return;

        transform.position = Vector3.MoveTowards(transform.position, new Vector3(TargetHex.transform.position.x, TargetHex.Position.y - 1, TargetHex.transform.position.z), Time.fixedDeltaTime * MoveSpeed);
        transform.LookAt(TargetHex.transform.position);
        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);

        if (Vector3.Distance(transform.position, new Vector3(TargetHex.transform.position.x, TargetHex.Position.y - 1, TargetHex.transform.position.z)) == 0)
        {
            if (AvailableHexes.Count > 1) CurrentTurnCount--;

            TargetHex.SetPickState(false);
            Position = TargetHex.Position;
            AvailableHexes.Remove(TargetHex);

            if (CurrentTurnCount == 0 || AvailableHexes.Count == 0)
            {
                EmitSignal(EnumMoveSignals.StopMoving);
                WorldUpdateSystem.Unsubscribe(this);
                CurrentTurnCount = TurnCount;
            }
        }
    }

    private void OnDestroy()
    {
        EventBus?.Unsubscribe(SignalBox);
        WorldUpdateSystem?.Unsubscribe(this);
    }
}