using System.Collections.Generic;
using System.Linq;

public class UnitPassport : BaseSignal
{
    EventBus EventBus;

    Unit Unit;

    public void Initialization(HashSet<object> Systems)
    {
        Unit = Systems.FirstOrDefault(x => x is Unit) as Unit;
        EventBus = Systems.FirstOrDefault(x => x is EventBus) as EventBus;
    }

    public void OnClick()
    {
        EventBus.Invoke(new PickUnitSignal(Unit));
    }
}