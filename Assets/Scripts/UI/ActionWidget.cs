using System.Collections.Generic;

public class ActionWidget : Widgets
{
    Unit Unit;

    HashSet<ActionButton> ActionButtons = new HashSet<ActionButton>();

    void Awake()
    {
        EventBus.Subscribe<PickUnitSignal>(SignalBox);

        ActionButtons.UnionWith(GetComponentsInChildren<ActionButton>());
        foreach (var ActionButton in ActionButtons)
        {
            ActionButton.Subscribe(SignalBox);
        }
    }

    protected override void SignalBox<T>(T Obj)
    {
        switch (Obj)
        {
            case PickUnitSignal PickUnitSignal :
                Enable(true);
                Unit = PickUnitSignal.Unit;
                break;
            case EnumButtonSignals EnumButtonSignals :
                EventBus.Invoke(new ActionSignal(Unit, EnumButtonSignals));
                break;
            default: break;
        }
    }

    void OnDestroy()
    {
        EventBus.Unsubscribe<PickUnitSignal>(SignalBox);
        foreach (var ActionButton in ActionButtons)
        {
            ActionButton.Unsubscribe(SignalBox);
        }
    }
}