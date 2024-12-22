using UnityEngine;

[System.Serializable]
public class HexPaintOption
{
    public MeshRenderer HexPrefab;
    public bool IsSelected;

    public string PrefabName => HexPrefab != null ? HexPrefab.name : "None";
}