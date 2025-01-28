using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class EventBusMonitor : MonoBehaviour
{
    [Inject] EventBus EventBus;
    public int TotalInvokeCount { get; private set; } = 0;
    public float CurrentInvokesPerSecond { get; private set; } = 0;
    public float MaxInvokesPerSecond { get; private set; } = 0;
    public Dictionary<string, int> EventBusData { get; private set; } = new Dictionary<string, int>();

    private float LastUpdateTime = 0;
    private int InvokesSinceLastUpdate = 0;

    private void Awake()
    {
        if (EventBus == null)
        {
            Debug.LogError("EventBusMonitor: EventBus is not assigned!");
        }
        else
        {
            EventBus.Update += IncrementInvokeCount;
        }
    }

    private void Update()
    {
        float DeltaTime = Time.time - LastUpdateTime;
        if (DeltaTime >= 1f)
        {
            CurrentInvokesPerSecond = InvokesSinceLastUpdate / DeltaTime;

            if (CurrentInvokesPerSecond > MaxInvokesPerSecond)
            {
                MaxInvokesPerSecond = CurrentInvokesPerSecond;
            }

            InvokesSinceLastUpdate = 0;
            LastUpdateTime = Time.time;
        }

        if (CurrentInvokesPerSecond > 0)
        {
            RefreshData();
        }
    }

    public void IncrementInvokeCount()
    {
        TotalInvokeCount++;
        InvokesSinceLastUpdate++;
    }

    public void ClearData()
    {
        TotalInvokeCount = 0;

        CurrentInvokesPerSecond = 0;

        MaxInvokesPerSecond = 0;
    }

    public void RefreshData()
    {
        if (EventBus == null) return;

        EventBusData.Clear();

        foreach (var Entry in EventBus.GetHandlersData())
        {
            string KeyName = Entry.Key.ToString();
            int SubscriberCount = Entry.Value;
            EventBusData[KeyName] = SubscriberCount;
        }
    }

    void OnDestroy() => EventBus.Update -= IncrementInvokeCount;
}