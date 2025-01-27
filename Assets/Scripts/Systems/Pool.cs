using System.Collections.Generic;
using UnityEngine;

public class Pool
{
    GameObject PoolFolder;
    private Dictionary<string, GameObject> ParentFolders = new Dictionary<string, GameObject>();

    public Pool()
    {
        PoolFolder = GameObject.FindGameObjectWithTag("Pool");

        if (!PoolFolder)
        {
            PoolFolder = new GameObject("Pool");
            PoolFolder.tag = "Pool";
            PoolFolder.transform.SetParent(GameObject.FindGameObjectWithTag("Systems").transform);
        }

        InitializeExistingChildren();
    }

    private void InitializeExistingChildren()
    {
        for (int i = 0; i < PoolFolder.transform.childCount; i++)
        {
            Transform child = PoolFolder.transform.GetChild(i);
            string typeName = child.name;

            if (!ParentFolders.ContainsKey(typeName))
            {
                ParentFolders[typeName] = child.gameObject;
            }

            int childCount = child.childCount;
            child.name = $"{typeName} [{childCount}]";
        }
    }

    public void Clear()
    {
        for (int i = 0; i < PoolFolder.transform.childCount; i++)
        {
            MonoBehaviour.Destroy(PoolFolder.transform.GetChild(i).gameObject);
        }

        ParentFolders.Clear();
    }

    public void Remove(Component obj)
    {
        string typeName = obj.GetType().Name;

        if (ParentFolders.TryGetValue(typeName, out GameObject parentFolder))
        {
            for (int i = 0; i < parentFolder.transform.childCount; i++)
            {
                Transform child = parentFolder.transform.GetChild(i);

                if (child.gameObject == obj.gameObject)
                {
                    MonoBehaviour.DestroyImmediate(child.gameObject);
                    break;
                }
            }

            int remainingCount = parentFolder.transform.childCount;

            if (remainingCount == 0)
            {
                ParentFolders.Remove(typeName);
                MonoBehaviour.DestroyImmediate(parentFolder);
            }
            else
            {
                parentFolder.name = $"{typeName} [{remainingCount}]";
            }
        }
        else
        {
            Debug.LogWarning($"Папка для типа {typeName} не найдена.");
        }
    }

    public void Add(Component obj)
    {
        string typeName = obj.GetType().Name;

        if (!ParentFolders.ContainsKey(typeName))
        {
            GameObject typeParent = new GameObject($"{typeName} [1]");
            typeParent.transform.SetParent(PoolFolder.transform);
            ParentFolders[typeName] = typeParent;
        }
        else
        {
            GameObject parentFolder = ParentFolders[typeName];
            int currentCount = parentFolder.transform.childCount + 1;
            parentFolder.name = $"{typeName} [{currentCount}]";
        }

        obj.transform.SetParent(ParentFolders[typeName].transform);
    }

    public HashSet<T> GetAllOfType<T>() where T : Component
    {
        string typeName = typeof(T).Name;

        if (ParentFolders.TryGetValue(typeName, out GameObject parentFolder))
        {
            T[] components = parentFolder.GetComponentsInChildren<T>(true);
            return new HashSet<T>(components);
        }
        else
        {
            Debug.LogWarning($"Нет объектов типа {typeName} в пуле.");
            return new HashSet<T>();
        }
    }
}