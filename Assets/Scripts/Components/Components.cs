using System.Collections.Generic;

public abstract class Components : BaseSignal
{
    public Unit Master {get; protected set;}

    public virtual void Initialization(Unit Master) => this.Master = Master;
    protected virtual void SignalBox<T>(T Obj){}
    protected virtual void ExtractSystems(HashSet<object> Systems){}
    protected virtual void ExtractComponents(HashSet<Components> Components){}
}