using System;
using System.Collections.Generic;

public class EventBus : ISystems
{
    private readonly Dictionary<object, List<Delegate>> EventHandlers = new Dictionary<object, List<Delegate>>();
    private object GlobalKey = typeof(object);

    public void Subscribe<T>(Action<T> Listener)
    {
        if (Listener == null) return;

        var key = typeof(T);
        if (!EventHandlers.ContainsKey(key))
        {
            EventHandlers[key] = new List<Delegate>();
        }

        if (!EventHandlers[key].Contains(Listener))
        {
            EventHandlers[key].Add(Listener);
        }
    }

    public void SubscribeToAll<T>(Action<T> Listener)
    {
        if (Listener == null) return;

        if (!EventHandlers.ContainsKey(GlobalKey))
        {
            EventHandlers[GlobalKey] = new List<Delegate>();
        }

        if (!EventHandlers[GlobalKey].Contains(Listener))
        {
            EventHandlers[GlobalKey].Add(Listener);
        }
    }

    public void Unsubscribe<T>(Action<T> Listener)
    {
        if (Listener == null) return;

        var key = typeof(T);
        if (EventHandlers.ContainsKey(key))
        {
            EventHandlers[key].RemoveAll(d => d == null || d.Equals(Listener));
            if (EventHandlers[key].Count == 0)
            {
                EventHandlers.Remove(key);
            }
        }
    }

    public void UnsubscribeFromAll<T>(Action<T> Listener)
    {
        if (Listener == null) return;

        foreach (var key in EventHandlers.Keys)
        {
            EventHandlers[key].RemoveAll(d => d == null || d.Equals(Listener));
        }
    }

    public void Invoke<T>(T payload)
    {
        var key = typeof(T);

        if (EventHandlers.ContainsKey(key))
        {
            EventHandlers[key].RemoveAll(d => d == null || d.Target == null);

            foreach (var Listener in EventHandlers[key])
            {
                if (Listener is Action<T> TypedListener)
                {
                    TypedListener.Invoke(payload);
                }
            }
        }

        if (EventHandlers.ContainsKey(GlobalKey))
        {
            EventHandlers[GlobalKey].RemoveAll(d => d == null || d.Target == null);

            foreach (var Listener in EventHandlers[GlobalKey])
            {
                if (Listener is Action<T> TypedListener)
                {
                    TypedListener.Invoke(payload);
                }
            }
        }
    }
}