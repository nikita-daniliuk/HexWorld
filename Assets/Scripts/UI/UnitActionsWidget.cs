public class UnitActionsWidget : Widgets
{
    Player Player;

    void Awake() => Subscribe();

    protected override void SignalBox(object Obj)
    {
        switch (Obj)
        {
            case Player Player :
                this.Player = Player;
                break;
            default: break;
        }
    }

    public void Jump()
    {
        EventBus.Invoke(new UnitJumpSignal(Player));
    }

    public void Walk()
    {
        EventBus.Invoke(new UnitWalkSignal(Player));
    }
}