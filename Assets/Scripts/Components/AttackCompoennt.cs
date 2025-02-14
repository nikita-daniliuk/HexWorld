using System.Collections.Generic;
using UnityEngine;

public class AttackCompoennt : Components
{
    EventBus EventBus;
    [SerializeField] float Damage;

    public override void Initialization(Unit Master)
    {
        ExtractSystems(Master.Systems);
    }

    protected override void ExtractSystems(HashSet<object> Systems)
    {
        foreach (var System in Systems)
        {
            switch (System)
            {
                case EventBus EventBus :
                    this.EventBus = EventBus;
                    this.EventBus.Subscribe<ActionSignal>(SignalBox);
                    break;
                default: break;
            }            
        }
    }

    protected override void SignalBox<T>(T Obj)
    {
        switch (Obj)
        {
            case ActionSignal ActionSignal :
                switch (ActionSignal.EnumButtonSignals)
                {
                    case EnumButtonSignals.Attack :
                        
                        break;
                    default: break;
                }
                break;
            default: break;
        }
    }

    void OnDestroy() => EventBus.Unsubscribe<ActionSignal>(SignalBox);
}