using UnityEngine;

public class PassportWidget : Widgets
{
    [SerializeField] UnitPassport UnitPassportPref;

    void Awake() => EventBus.Subscribe<UnitRegPassportSignal>(SignalBox);

    protected override void SignalBox<T>(T Obj)
    {
        switch (Obj)
        {
            case UnitRegPassportSignal PassportSignal :
                var Passport = Instantiate(UnitPassportPref, Widget.transform);
                Passport.Initialization(new System.Collections.Generic.HashSet<object> {
                    EventBus,
                    PassportSignal.Unit
                });
                break;
            default: break;
        }
    }

    void OnDestroy() => EventBus.UnsubscribeFromAll<object>(SignalBox);
}