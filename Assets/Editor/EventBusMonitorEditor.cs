using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EventBusMonitor))]
public class EventBusMonitorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        EventBusMonitor Monitor = (EventBusMonitor)target;

        EditorGUILayout.LabelField("EventBus Monitor", EditorStyles.boldLabel);

        EditorGUILayout.LabelField("Total Invokes:", Monitor.TotalInvokeCount.ToString());
        EditorGUILayout.LabelField("Current Invokes Per Second:", Monitor.CurrentInvokesPerSecond.ToString("F2"));
        EditorGUILayout.LabelField("Max Invokes Per Second:", Monitor.MaxInvokesPerSecond.ToString("F2"));

        EditorGUILayout.Space();

        if (Monitor.EventBusData != null && Monitor.EventBusData.Count > 0)
        {
            EditorGUILayout.LabelField("Event Handlers Data:", EditorStyles.boldLabel);
            foreach (var entry in Monitor.EventBusData)
            {
                EditorGUILayout.LabelField($"Key: {entry.Key}", $"Subscribers: {entry.Value}");
            }
        }
        else
        {
            EditorGUILayout.LabelField("Event Handlers Data: No active handlers");
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Refresh Data"))
        {
            Monitor.ClearData();
        }

        Repaint();
    }
}