using UnityEngine;
using Zenject;
using System.Collections.Generic;
using System.Linq;

public class PlayerSpawner : MonoBehaviour
{
    [Inject] EventBus EventBus;
    [Inject] Factory Factory;
    [Inject] WorldUpdateSystem WorldUpdateSystem;
    [Inject] Pool Pool;

    [SerializeField] Unit PlayerPrefab;

    void Start() => Invoke(nameof(Spawn), 1f);

    void Spawn()
    {
        Hex RandomHex = Pool.GetAllOfType<Hex>()
            .Where(x => x.IsWalkable)
            .OrderBy(x => UnityEngine.Random.value)
            .FirstOrDefault();

        if(!RandomHex) return;

        var Unit = Factory.Create<Unit>(PlayerPrefab.gameObject, RandomHex.transform.position);

        Unit.Initialization(new HashSet<object>{
            EventBus,
            WorldUpdateSystem
        }); 

        Unit.GetComponentByType<MoveComponent>().Position = RandomHex.Position;

        RandomHex.SetIsWalkable(false);            
    }
}