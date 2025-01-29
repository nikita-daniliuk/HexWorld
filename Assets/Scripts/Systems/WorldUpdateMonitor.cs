using UnityEngine;
using Zenject;
using System.Collections.Generic;

public sealed class WorldUpdateMonitor : MonoBehaviour
{
    #if UNITY_EDITOR
    [SerializeField, Tooltip("Number of objects in FixedUpdate")]
    private int FixedUpdateCount;

    [Inject] WorldUpdateSystem WorldUpdateSystem;

    private readonly HashSet<object> SubscribedObjects = new HashSet<object>();

    void Start() => WorldUpdateSystem.Update += OnWorldUpdate;

    private void OnWorldUpdate(int Count)
    {
        FixedUpdateCount = Count;
        UpdateSubscribedObjects();
    }

    private void UpdateSubscribedObjects()
    {
        SubscribedObjects.Clear();

        if (WorldUpdateSystem != null)
        {
            SubscribedObjects.UnionWith(WorldUpdateSystem.GetSubscribedObjects());
        }
    }

    public int GetFixedUpdateCount() => FixedUpdateCount;

    public HashSet<object> GetSubscribedObjects() => SubscribedObjects;

    private void OnDestroy()
    {
        if (WorldUpdateSystem != null)
        {
            WorldUpdateSystem.Update -= OnWorldUpdate;
        }
    }
    #endif
}