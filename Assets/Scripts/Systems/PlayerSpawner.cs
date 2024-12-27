using UnityEngine;
using Zenject;
using System.Collections.Generic;
using System.Linq;

public class PlayerSpawner : MonoBehaviour
{
    [Inject] EventBus EventBus;
    [Inject] Factory Factory;
    [Inject] WorldUpdateSystem WorldUpdateSystem;
    [Inject] StepByStepSystem StepByStepSystem;
    [Inject] Pool Pool;

    [SerializeField] Unit PlayerPrefab;

    void Start() => Spawn();

    void SignalBox(object Obj)
    {
        switch (Obj)
        {
            case EnumGenerateSignals.StopGeneration :
                Spawn();
                break;
            default: break;
        }
    }

    void Spawn()
    {
        Hex RandomHex = Pool.GetAllOfType<Hex>()
            .Where(x => x.IsWalkable)
            .OrderBy(x => UnityEngine.Random.value)
            .FirstOrDefault();

        if(!RandomHex) return;

        var Player = Factory.Create<Unit>(PlayerPrefab.gameObject, RandomHex.transform.position);

        Player.Initialization(new HashSet<object>{
            EventBus,
            WorldUpdateSystem,
            StepByStepSystem
        }); 

        Player.GetComponentByType<MoveComponent>().Position = RandomHex.Position;
    }
}