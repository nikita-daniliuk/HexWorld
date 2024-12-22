using System.Collections.Generic;
using System.Diagnostics;
using Zenject;

public sealed class WorldUpdateSystem : IFixedTickable, ISystems
{
    private readonly HashSet<IFixedUpdate> fixedUpdatesObj = new HashSet<IFixedUpdate>();

    private readonly List<IFixedUpdate> toRemoveFixed = new List<IFixedUpdate>();
    private readonly List<IFixedUpdate> toAddFixed = new List<IFixedUpdate>();

    private bool isUpdating;

    public void FixedTick()
    {
        isUpdating = true;
        ProcessPendingUpdates(fixedUpdatesObj, toAddFixed, toRemoveFixed, obj => obj.FixedRefresh());
        isUpdating = false;
    }

    private void ProcessPendingUpdates<T>(HashSet<T> mainSet, List<T> toAdd, List<T> toRemove, System.Action<T> action)
    {
        if (isUpdating)
        {
            if (toAdd.Count > 0)
            {
                mainSet.UnionWith(toAdd);
                toAdd.Clear();
            }

            if (toRemove.Count > 0)
            {
                mainSet.ExceptWith(toRemove);
                toRemove.Clear();
            }
        }
        else
        {
            foreach (var obj in toAdd)
            {
                mainSet.Add(obj);
            }
            toAdd.Clear();

            foreach (var obj in toRemove)
            {
                mainSet.Remove(obj);
            }
            toRemove.Clear();
        }

        foreach (var obj in mainSet)
        {
            action(obj);
        }
    }

    public void Subscribe(IFixedUpdate obj)
    {
        if (isUpdating)
        {
            toAddFixed.Add(obj);
        }
        else
        {
            fixedUpdatesObj.Add(obj);
        }
    }


    public void Unsubscribe(IFixedUpdate obj)
    {
        if (isUpdating)
        {
            toRemoveFixed.Add(obj);
        }
        else
        {
            fixedUpdatesObj.Remove(obj);
        }
    }

    public bool IsSubscribe(IFixedUpdate obj) => fixedUpdatesObj.Contains(obj);
}