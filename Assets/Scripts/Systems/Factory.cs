using UnityEngine;

public class Factory : ISystems
{
    private Pool Pool;

    public Factory(Pool Pool)
    {
        this.Pool = Pool;
    }

    public T Create<T>(GameObject Prefab, Vector3 Position, Quaternion Rotation) where T : Component
    {
        GameObject Instance = MonoBehaviour.Instantiate(Prefab, Position, Rotation);
        var Component = Instance.GetComponent<T>();
        Pool.Add(Component);

        return Component;
    }

    public T Create<T>(GameObject Prefab, Vector3 Position) where T : Component
    {
        return Create<T>(Prefab, Position, Quaternion.identity);
    }

    public T Create<T>(GameObject Prefab) where T : Component
    {
        return Create<T>(Prefab, Vector3.zero, Quaternion.identity);
    }
}