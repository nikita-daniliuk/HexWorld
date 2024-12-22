using System.Collections.Generic;

public class EventBus : ISystems
{
    HashSet<object> ToAll = new HashSet<object>();

    public delegate void Action(object obj);
    private event Action Event;

    public void Invoke(object obj)
    {
        Event?.Invoke(obj);
    }

    public void Subscribe(Action listener)
    {
        Event += listener;

        for (int i = 0; i < ToAll.Count; i++)
        {
            listener(i);
        }
    }

    public void Unsubscribe(Action listener)
    {
        Event -= listener;
    }

    public void SendToAll(object Obj)
    {
        ToAll.Add(Obj);
    }
}