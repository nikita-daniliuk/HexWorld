using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class HexWalkable : BaseSignal
{
    private Transform HexParent;
    [SerializeField, HideInInspector] private List<Hex> AllHexes = new List<Hex>();

    bool IsShowing;

    private bool FindHexParent()
    {
        var PoolParent = GameObject.FindGameObjectWithTag("Pool");

        if (PoolParent == null) return false;

        HexParent = PoolParent.transform.Find("Hex")?.transform;

        return HexParent != null;
    }

    public void Refresh() => AllHexes.Clear();

    public void ShowHexWalkableMap()
    {
        if(!IsShowing)
        {
            FindHexParent();
            AllHexes = HexParent.GetComponentsInChildren<Hex>().ToList();

            foreach (var Hex in AllHexes) SetWalkableMap(Hex);    
            IsShowing = true;        
        }
    }

    public void SetWalkableMap(Hex Hex)
    {
        Hex.Walkable.gameObject.SetActive(Hex.IsWalkable);
        Hex.NotWalkable.gameObject.SetActive(!Hex.IsWalkable);
    }

    public void HideHexWalkableMap()
    {
        foreach (var Hex in AllHexes)
        {
            Hex.Walkable.gameObject.SetActive(false);
            Hex.NotWalkable.gameObject.SetActive(false);
        }   

        IsShowing = false;       
    }
}