using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MoveComponent : Components, IFixedUpdate
{
    EventBus EventBus;
    WorldUpdateSystem WorldUpdateSystem;
    StepByStepSystem StepByStepSystem;
    [ReadOnly] public Vector3Int Position;

    [SerializeField] float MoveSpeed;

    [SerializeField] private int _MaxStepsPerTurn;
    public int MaxStepsPerTurn => _MaxStepsPerTurn;

    [SerializeField, ReadOnly] private int _CurrentTurnCount;
    public int CurrentTurnCount => _CurrentTurnCount;

    private HashSet<Hex> AvailableHexes = new HashSet<Hex>();

    Hex TargetHex;

    void OnValidate()
    {
        _MaxStepsPerTurn = Mathf.Max(0, _MaxStepsPerTurn);
        _CurrentTurnCount = _MaxStepsPerTurn;
        MoveSpeed = Mathf.Max(0, MoveSpeed);
    }

    public override void Initialization(Unit Master)
    {
        _CurrentTurnCount = _MaxStepsPerTurn;
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
                _CurrentTurnCount = _MaxStepsPerTurn;
                break;

            default: break;
        }
    }

    public void FixedRefresh() => Move();

    private void Move()
    {
        if(TargetHex == null)
        {
            TargetHex = AvailableHexes.FirstOrDefault();
            if(!TargetHex) return;
        }

        transform.position = Vector3.MoveTowards(transform.position, new Vector3(TargetHex.transform.position.x, TargetHex.Position.y - 1, TargetHex.transform.position.z), Time.fixedDeltaTime * MoveSpeed);
        transform.LookAt(TargetHex.transform.position);
        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);

        if (Vector3.Distance(transform.position, new Vector3(TargetHex.transform.position.x, TargetHex.Position.y - 1, TargetHex.transform.position.z)) == 0)
        {
            _CurrentTurnCount--;
            TargetHex.SetPickState(false);
            Position = TargetHex.Position;
            AvailableHexes.Remove(TargetHex);
            TargetHex = null;

            if (CurrentTurnCount == 0 || AvailableHexes.Count == 0)
            {
                EmitSignal(EnumMoveSignals.StopMoving);
                WorldUpdateSystem.Unsubscribe(this);
                _CurrentTurnCount = _MaxStepsPerTurn;
            }
        }
    }

    private void OnDestroy()
    {
        EventBus?.Unsubscribe(SignalBox);
        WorldUpdateSystem?.Unsubscribe(this);
    }
}