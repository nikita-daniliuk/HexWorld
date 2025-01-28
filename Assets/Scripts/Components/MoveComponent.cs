using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;

public class MoveComponent : Components, IFixedUpdate
{
    EventBus EventBus;
    WorldUpdateSystem WorldUpdateSystem;
    [ReadOnly] public Vector3Int Position;

    [SerializeField] float MoveSpeed;

    [SerializeField] private int _JumpLength;
    public int JumpLength => _JumpLength;

    [SerializeField] private int _MaxStepsPerTurn;
    public int MaxStepsPerTurn => _MaxStepsPerTurn;

    [SerializeField, ReadOnly] private int _CurrentTurnCount;
    public int CurrentTurnCount => _CurrentTurnCount;

    private HashSet<Hex> AvailableHexes = new HashSet<Hex>();

    public Rigidbody RB {get; private set;}
    public Collider Collider {get; private set;}

    [SerializeField] private float FlightSpeed = 10f;
    [SerializeField] private float JumpImpulseHeight = 2f;

    Hex TargetHex;

    void OnValidate()
    {
        _MaxStepsPerTurn = Mathf.Max(0, _MaxStepsPerTurn);
        _CurrentTurnCount = _MaxStepsPerTurn;
        MoveSpeed = Mathf.Max(0, MoveSpeed);
    }

    public override void Initialization(Unit Master)
    {
        base.Initialization(Master);
        
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
                    EventBus.Subscribe<PickUnitSignal>(SignalBox);
                    break;
                case WorldUpdateSystem WorldUpdateSystem:
                    this.WorldUpdateSystem = WorldUpdateSystem;
                    break;
                case Rigidbody RB :
                    this.RB = RB;
                    break; 
                case Collider Collider :
                    this.Collider = Collider;
                    break;
                default: break;
            }
        }
    }

    protected override void SignalBox<T>(T Obj)
    {
        switch (Obj)
        {
            case PickUnitSignal PickUnitSignal :
                if(PickUnitSignal.Unit.State != EnumUnitState.Move)
                {
                    EmitSignal(EnumMoveSignals.StopMoving);
                }
                break;
            default: break;
        }
    }

    public void ReadyToJump() => EmitSignal(EnumMoveSignals.StartJump); 

    public void SetNewPath(HashSet<Hex> Path)
    {
        switch (Master.State)
        {
            case EnumUnitState.Stay :
                EmitSignal(EnumMoveSignals.StartMoving);
                var StartHex = Path.FirstOrDefault(x => x.Position == Position);
                StartHex.SetIsWalkable(true); 
                StartHex.SetPickState(false);
                Path.Remove(StartHex);
                AvailableHexes.Clear();
                AvailableHexes.UnionWith(Path);
                WorldUpdateSystem.Subscribe(this);                 
                break;
            case EnumUnitState.Jump :
                var Hex = Path.FirstOrDefault();
                Path.FirstOrDefault(x => x.Position != Hex.Position).SetIsWalkable(true);
                StartCoroutine(SimulateJumpWithoutPhysics(transform.position, Hex.transform.position, FlightSpeed));
                Position = Hex.Position;
                Hex.SetIsWalkable(false);     
                break;
            default: break;
        }
    }

    public void FixedRefresh() => Move();

    public bool CanJumpToTarget(Vector3 Target)
    {
        if (!(Collider is CapsuleCollider CapsuleCollider))
        {
            Debug.LogError("Collider is not a CapsuleCollider! Cannot Check Path.");
            return false;
        }

        Vector3 ColliderOffset = CapsuleCollider.center;
        float Radius = CapsuleCollider.radius;
        float Height = CapsuleCollider.height;

        List<Vector3> TrajectoryPoints = GenerateTrajectory(transform.position, Target);

        foreach (Vector3 Point in TrajectoryPoints)
        {
            Vector3 CapsuleStart = Point + ColliderOffset + new Vector3(0, Radius, 0);
            Vector3 CapsuleEnd = Point + ColliderOffset + new Vector3(0, Height - Radius * 2, 0);

            Collider[] HitColliders = Physics.OverlapCapsule(
                CapsuleStart,
                CapsuleEnd,
                Radius,
                ~0,
                QueryTriggerInteraction.Ignore
            );

            foreach (var Hit in HitColliders)
            {
                if (Hit != Collider)
                {
                    return false;
                }
            }
        }

        return true;
    }

    List<Vector3> GenerateTrajectory(Vector3 Start, Vector3 Target)
    {
        List<Vector3> TrajectoryPoints = new List<Vector3>();

        GameObject TempObject = new GameObject("TrajectorySimulator");
        Rigidbody TempRb = TempObject.AddComponent<Rigidbody>();

        TempObject.transform.position = Start;
        TempRb.isKinematic = false;
        TempRb.useGravity = true;

        Vector3 Direction = Target - Start;

        float MaxHeight = Mathf.Max(Start.y + JumpImpulseHeight, Target.y + JumpImpulseHeight);

        float TimeToMaxHeight = Mathf.Sqrt(2 * (MaxHeight - Start.y) / -Physics.gravity.y);
        float TimeToFall = Mathf.Sqrt(2 * (MaxHeight - Target.y) / -Physics.gravity.y);
        float TotalTime = TimeToMaxHeight + TimeToFall;

        float Vx = Direction.x / TotalTime;
        float Vz = Direction.z / TotalTime;

        float Vy = Mathf.Sqrt(2 * -Physics.gravity.y * (MaxHeight - Start.y));

        Vector3 InitialVelocity = new Vector3(Vx, Vy, Vz);
        TempRb.velocity = InitialVelocity;

        float SimulationTime = 0f;
        while (SimulationTime < TotalTime)
        {
            TrajectoryPoints.Add(TempObject.transform.position);

            TempRb.velocity += Physics.gravity * Time.fixedDeltaTime;
            TempObject.transform.position += TempRb.velocity * Time.fixedDeltaTime;

            SimulationTime += Time.fixedDeltaTime;
        }

        if (TrajectoryPoints.Last() != Target) TrajectoryPoints.Add(Target);

        Destroy(TempObject);

        return TrajectoryPoints;
    }

    IEnumerator SimulateJumpWithoutPhysics(Vector3 Start, Vector3 Target, float SimulationSpeed)
    {
        List<Vector3> trajectoryPoints = GenerateTrajectory(Start, Target);

        transform.LookAt(trajectoryPoints[trajectoryPoints.Count - 1]);

        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);

        foreach (Vector3 Point in trajectoryPoints)
        {
            transform.position = Point;
            yield return new WaitForSeconds(Time.fixedDeltaTime / SimulationSpeed);
        }

        EmitSignal(EnumMoveSignals.StopJump);
    }

    void Move()
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
            if(AvailableHexes.Count == 1) TargetHex.SetIsWalkable(false);
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

    void OnDestroy()
    {
        EventBus.Unsubscribe<PickUnitSignal>(SignalBox);
        WorldUpdateSystem?.Unsubscribe(this);
    }
}