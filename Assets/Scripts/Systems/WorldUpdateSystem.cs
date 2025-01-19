using System.Collections.Generic;
using System;
using Zenject;

public sealed class WorldUpdateSystem : IFixedTickable
{
    private readonly HashSet<IFixedUpdate> FixedUpdatesObj = new HashSet<IFixedUpdate>();
    private readonly List<IFixedUpdate> ToRemoveFixed = new List<IFixedUpdate>();
    private readonly List<IFixedUpdate> ToAddFixed = new List<IFixedUpdate>();
    private bool IsUpdating;

    public void FixedTick()
    {
        IsUpdating = true;
        ProcessPendingUpdates(FixedUpdatesObj, ToAddFixed, ToRemoveFixed, Obj => Obj.FixedRefresh());
        IsUpdating = false;
    }

    private void ProcessPendingUpdates<T>(HashSet<T> MainSet, List<T> ToAdd, List<T> ToRemove, Action<T> Action)
    {
        if (ToAdd.Count > 0)
        {
            MainSet.UnionWith(ToAdd);
            ToAdd.Clear();
        }

        List<T> ToRemoveNow = new List<T>();

        foreach (var Obj in MainSet)
        {
            Action(Obj);

            if (ToRemove.Contains(Obj))
            {
                ToRemoveNow.Add(Obj);
            }
        }

        foreach (var Obj in ToRemoveNow)
        {
            MainSet.Remove(Obj);
        }

        ToRemove.Clear();
    }

    public void Subscribe(IFixedUpdate Obj)
    {
        if (ToRemoveFixed.Contains(Obj))
        {
            ToRemoveFixed.Remove(Obj);
        }

        if (IsUpdating)
        {
            if (!ToAddFixed.Contains(Obj))
                ToAddFixed.Add(Obj);
        }
        else
        {
            FixedUpdatesObj.Add(Obj);
        }
    }

    public void Unsubscribe(IFixedUpdate Obj)
    {
        if (IsUpdating)
        {
            if (!ToRemoveFixed.Contains(Obj))
                ToRemoveFixed.Add(Obj);
        }
        else
        {
            FixedUpdatesObj.Remove(Obj);
        }
    }

    public bool IsSubscribe(IFixedUpdate Obj) => FixedUpdatesObj.Contains(Obj);
}