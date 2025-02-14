using UnityEngine;

public class ActionButton : BaseSignal
{
    [SerializeField] EnumButtonSignals ButtonSignal;

    public void Invoke() => EmitSignal(ButtonSignal);
}
