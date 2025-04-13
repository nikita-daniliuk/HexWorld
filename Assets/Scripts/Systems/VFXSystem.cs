using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class VFXSystem : MonoBehaviour
{
    [Inject] private Factory Factory;
    [Inject] private EventBus EventBus;
    [Inject] private Pool Pool;
    [SerializeField] private List<VFX> VFXPrefabs;

    private void Start()
    {
        EventBus.Subscribe<VFXSignal>(OnVFXSignal);
    }

    private void OnVFXSignal(VFXSignal Signal)
    {
        CreateVFXByType(Signal);
    }

    private void CreateVFXByType(VFXSignal Signal)
    {
        VFX Prefab = VFXPrefabs.Find(x => x.GetType() == Signal.VFXType);
        if (Prefab != null)
        {
            var AllVFX = Pool.GetAllOfType<VFX>();

            var FindVFX = AllVFX.FirstOrDefault(x => x.GetType().IsAssignableFrom(Signal.VFXType) && !x.gameObject.activeSelf);

            if (FindVFX != null)
            {
                FindVFX.transform.position = Signal.Position;
                FindVFX.gameObject.SetActive(true);
            }
            else
            {
                Factory.Create<VFX>(Prefab.gameObject, Signal.Position, Quaternion.identity);
            }
            
        }
        else
        {
            Debug.LogWarning($"VFX префаб для типа {Signal.VFXType} не найден.");
        }
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<VFXSignal>(OnVFXSignal);
    }
}