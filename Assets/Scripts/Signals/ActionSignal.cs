public class ActionSignal
{
    public readonly Unit Unit;
    public readonly EnumButtonSignals EnumButtonSignals;

    public ActionSignal(Unit Unit, EnumButtonSignals EnumButtonSignals)
    {
        this.Unit = Unit;
        this.EnumButtonSignals = EnumButtonSignals;
    }
}