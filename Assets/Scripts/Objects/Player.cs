using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player : Unit
{
    public override void Initialization(HashSet<object> Systems)
    {
        this.Systems.UnionWith(Systems);
        this.Systems.Add(GetComponent<Rigidbody>());
        this.Systems.Add(GetComponent<Collider>());
        Components = GetComponents<Components>().ToHashSet();
        InitComponents();     
        SubsribeOnComponentsSignals();
        
        GetSystemByType<EventBus>().Invoke(new UnitRegPassportSignal(this));
    }

    protected override void SignalBox<T>(T Obj)
    {
        switch (Obj)
        {
            case EnumMoveSignals MoveSignal:
                switch (MoveSignal)
                {   
                    case EnumMoveSignals.StartMoving :
                        ChangeState(EnumUnitState.Move);
                        break;
                    case EnumMoveSignals.StopJump :
                    case EnumMoveSignals.StopMoving :
                        ChangeState(EnumUnitState.Stay);
                        break;
                    case EnumMoveSignals.StartJump :
                        ChangeState(EnumUnitState.Jump);
                        break;
                    default: break;
                }
                break;
            default: break;
        }

        EmitSignal(Obj);
    }
}