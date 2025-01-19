using UnityEngine;
using Zenject;

public abstract class Widgets : BaseSignal         
{
    [Inject] protected EventBus EventBus;
    [SerializeField] protected GameObject Widget;
    
    protected virtual void Enable(bool Switch)
    {
        Widget.SetActive(Switch);
    }

    protected virtual void SignalBox<T>(T Obj){}

    void OnDestroy()
    {
        EventBus?.UnsubscribeFromAll<object>(SignalBox);
    }
}