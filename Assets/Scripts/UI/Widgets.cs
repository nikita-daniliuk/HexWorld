using UnityEngine;
using Zenject;

public abstract class Widgets : BaseSignal         
{
    [Inject] protected EventBus EventBus;
    [SerializeField] protected GameObject Widget;

    protected virtual void Subscribe()
    {
        EventBus.Subscribe(SignalBox);
    }
    
    protected virtual void Unsubscribe()
    {
        EventBus.Unsubscribe(SignalBox);
    }
    
    protected virtual void Enable(bool Switch)
    {
        Widget.SetActive(Switch);
    }

    protected virtual void SignalBox(object Obj){}

    void OnDestroy()
    {
        Unsubscribe();
    }
}