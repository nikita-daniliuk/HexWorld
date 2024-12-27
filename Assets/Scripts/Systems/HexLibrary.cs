using System.Collections.Generic;
using UnityEngine;

public static class HexLibrary
{
    public static List<Vector3Int> GetHexDirections() => new List<Vector3Int>
    {
        new Vector3Int( 1, 0,  0),
        new Vector3Int( 0, 0,  1),
        new Vector3Int(-1, 0,  1),
        new Vector3Int(-1, 0,  0),
        new Vector3Int( 0, 0, -1),
        new Vector3Int( 1, 0, -1)
    };
}