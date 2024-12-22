using System.Collections.Generic;

public abstract class Components : BaseSignal
{
    public abstract void Initialization(Unit Master);

    protected virtual void SignalBox(object Obj){}

    protected virtual void ExtractSystems(HashSet<object> Systems){}
    protected virtual void ExtractComponents(HashSet<Components> Components){}
}