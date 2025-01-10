using Zenject;

public class UnitCard : BaseSignal
{
    [Inject] EventBus EventBus;

    Player Player;

    void Awake() => EventBus.Subscribe(SignalBox);

    void SignalBox(object Obj)
    {
        switch (Obj)
        {
            case Player Player :
                this.Player = Player;
                break;
            default: break;
        }
    }

    public void OnClick()
    {
        EventBus.Invoke(new PickUnitSignal(Player));
    }

    void OnDestroy() => EventBus.Unsubscribe(SignalBox);
}