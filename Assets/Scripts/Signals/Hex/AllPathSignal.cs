using System.Collections.Generic;

public struct AllPathSignal
{
    public readonly List<Hex> PossiblePositions;

    public AllPathSignal(List<Hex> PossiblePositions)
    {
        this.PossiblePositions = PossiblePositions;
    }
}