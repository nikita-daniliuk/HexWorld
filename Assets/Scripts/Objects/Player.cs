using System.Collections.Generic;
using System.Linq;

public class Player : Unit
{
    public override void Initialization(HashSet<object> Systems)
    {
        this.Systems.UnionWith(Systems);
        Components = GetComponents<Components>().ToHashSet();
        InitComponents();     
        SubsribeOnComponentsSignals();
    }

    protected override void SignalBox(object Obj)
    {
        switch (Obj)
        {
            case EnumMoveSignals MoveSignal:
                switch (MoveSignal)
                {   
                    case EnumMoveSignals.StartMoving :
                        ChangeState(EnumUnitState.Move);
                        break;
                    case EnumMoveSignals.StopMoving :
                        ChangeState(EnumUnitState.Stay);
                        break;
                    default: break;
                }
                break;
            default: break;
        }
    }

    void OnMouseDown()
    {
        GetSystemByType<EventBus>().Invoke(new PickUnitSignal(this));
    }
}