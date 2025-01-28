using UnityEngine;

public class AnimatorComponent : Components
{
    [SerializeField] Animator Animator;

    public override void Initialization(Unit Master)
    {
        base.Initialization(Master);
        
        this.Master = Master;
        Master.Subscribe(SignalBox);
    }

    protected override void SignalBox<T>(T Obj)
    {
        switch (Obj)
        {
            case EnumMoveSignals.StartMoving :
                Animator.SetInteger("State", (int)EnumAnimatorStates.Run); 
                break;
            case EnumMoveSignals.StopMoving :
                Animator.SetInteger("State", (int)EnumAnimatorStates.Idle);
                break;
            default: break;
        }
    }

    void OnDestroy() => Master.Unsubscribe(SignalBox);
}

public enum EnumAnimatorStates
{
    Idle, Run
}