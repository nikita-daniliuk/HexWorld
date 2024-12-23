using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#nullable enable

public abstract class Unit : BaseSignal
{
    [SerializeField, ReadOnly] protected EnumUnitState _State;
    public EnumUnitState State => _State;

    public HashSet<object> Systems = new HashSet<object>();
    public HashSet<Components> Components = new HashSet<Components>();

    public virtual void Initialization(HashSet<object> Systems)
    {
        EmitSignal(new Message(EnumUnitSignals.Initialization, gameObject, gameObject.name));
    }

    protected virtual void ChangeState(EnumUnitState State)
    {
        _State = State;
        EmitSignal(EnumUnitSignals.StateUpdated);
    } 

    public T? GetComponentByType<T>() where T : Components
    {
        foreach (var Component in Components)
        {
            if (Component is T)
            {
                return Component as T;
            }
        }
        return null;
    }

    public T? GetSystemByType<T>() where T : notnull
    {
        return Systems.OfType<T>().FirstOrDefault();
    }

    protected virtual void SubsribeOnComponentsSignals()
    {
        foreach (var Component in Components)
        {
            Component?.Subscribe(SignalBox);
        }
    }

    protected virtual void UnsubscribeOnComponentsSignals()
    {
        foreach (var Component in Components)
        {
            Component?.Unsubscribe(SignalBox);
        }
    }

    protected virtual void InitComponents()
    {
        foreach (var Component in Components)
        {
            Component.Initialization(this);
        }
    }

    protected virtual void ExtractSystems(HashSet<object> Systems){}
    protected virtual void ExtractComponents(HashSet<Components> Components){}

    protected virtual void SignalBox(object Obj){}

    void OnDestroy() => UnsubscribeOnComponentsSignals();
}