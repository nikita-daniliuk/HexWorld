using System.Collections.Generic;
using UnityEngine;

public class HexPainter : MonoBehaviour
{
    public void HandlePainting(Hex Hex, List<HexPaintOption> HexPaintOptions)
    {
        if (!Hex.HexVisual.enabled)
        {
            Hex.HexVisual.enabled = true;
            Hex.SetIsWalkable(false);
        }

        MeshRenderer SelectedHex = null;
        foreach (var Option in HexPaintOptions)
        {
            if (Option.IsSelected && Option.HexPrefab != null)
            {
                SelectedHex = Option.HexPrefab;
                break;
            }
        }

        if (SelectedHex != null && SelectedHex.sharedMaterial != Hex.HexVisual.sharedMaterial)
        {
            Hex.SetHexVisual(SelectedHex);
        }
    }
}