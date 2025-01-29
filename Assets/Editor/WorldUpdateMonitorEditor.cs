#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(WorldUpdateMonitor))]
public class WorldUpdateMonitorEditor : Editor
{
    private bool ShowSubscribedObjects = false;

    public override void OnInspectorGUI()
    {
        WorldUpdateMonitor Monitor = (WorldUpdateMonitor)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("World Update Monitor", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Objects in FixedUpdate:", Monitor ? Monitor.GetFixedUpdateCount().ToString() : "N/A");

        ShowSubscribedObjects = EditorGUILayout.Foldout(ShowSubscribedObjects, "Subscribed Objects");

        if (ShowSubscribedObjects)
        {
            HashSet<object> SubscribedObjects = Monitor.GetSubscribedObjects();

            if (SubscribedObjects.Count == 0)
            {
                EditorGUILayout.LabelField("No objects subscribed.");
            }
            else
            {
                foreach (var Obj in SubscribedObjects)
                {
                    if (Obj is MonoBehaviour MonoBehaviour)
                    {
                        if (GUILayout.Button($"{MonoBehaviour.name} ({Obj.GetType()})"))
                        {
                            Selection.activeObject = MonoBehaviour.gameObject;
                        }
                    }
                    else
                    {
                        EditorGUILayout.LabelField(Obj.GetType().Name);
                    }
                }
            }
        }

        Repaint();
    }
}
#endif