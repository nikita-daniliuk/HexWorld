using System.Collections.Generic;

public class PathSignal
{
    public readonly HashSet<Hex> Hexes = new HashSet<Hex>();

    public PathSignal(HashSet<Hex> Hexes)
    {
        this.Hexes = Hexes;
    }
}