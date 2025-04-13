using System.Linq;
using UnityEngine;

public class Factory
{
    private Pool Pool;

    public Factory(Pool Pool, EventBus EventBus)
    {
        this.Pool = Pool;
        EventBus.Invoke(this);
    }

    public T Create<T>(GameObject Prefab, Vector3 Position, Quaternion Rotation) where T : Component
    {
        if (Prefab == null)
        {
            Debug.LogError("Prefab cannot be null.");
            return null;
        }
        else
        {
            var Object = Pool.GetAllOfType<T>().FirstOrDefault(x => !x.gameObject.activeSelf);

            if(Object)
            {
                Object.transform.position = Position;
                Object.transform.rotation = Rotation;
                Object.gameObject.SetActive(true);
                return Object.GetComponent<T>();
            }
            else
            {
                GameObject Instance = MonoBehaviour.Instantiate(Prefab, Position, Rotation);
                var Component = Instance.GetComponent<T>();
                Pool.Add(Component);  
                return Component;              
            }
        }
    }

    public T Create<T>(GameObject Prefab, Vector3 Position) where T : Component
    {
        return Create<T>(Prefab, Position, Quaternion.identity);
    }

    public T Create<T>(GameObject Prefab) where T : Component
    {
        return Create<T>(Prefab, Vector3.zero, Quaternion.identity);
    }

    public T CreateFromInstance<T>(GameObject Instance) where T : Component
    {
        if (Instance == null)
        {
            Debug.LogWarning("Instance cannot be null.");
            return null;
        }

        var Component = Instance.GetComponent<T>();
        if (Component != null)
        {
            Pool.Add(Component);
        }
        else
        {
            Debug.LogError($"No component of type {typeof(T)} found on the provided GameObject.");
        }

        return Component;
    }

    public T CreateByType<T>() where T : Component
    {
        var Object = Pool.TryGetDeactiveObjByType<T>();

        if(Object)
        {
            Object.gameObject.SetActive(true);
            return Object;
        }
        else
        {
            GameObject NewObject = new GameObject();
            var Component = NewObject.AddComponent<T>();
            Pool.Add(Component);
            return Component;
        }
    }
}