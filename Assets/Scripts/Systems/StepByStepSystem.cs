public class StepByStepSystem : ISystems
{
    EventBus EventBus;

    public int CurrentTeam {get; private set;}

    public StepByStepSystem(EventBus EventBus)
    {
        this.EventBus = EventBus;
    }

    public void ChangeTurn()
    {
        CurrentTeam++;
        EventBus.Invoke(EnumSignals.NextTurn);
    }

    public void ChangeTurn(int TeamNumber)
    {
        CurrentTeam = TeamNumber;
        EventBus.Invoke(EnumSignals.NextTurn);
    }

    public bool IsItMyTurn(int TeamNumber)
    {
        // switch (TeamNumber)
        // {
        //     case 0 :
        //         return CurrentPlayerTurn == EnumPlayers.Second;
        //     case 1 :
        //         return CurrentPlayerTurn == EnumPlayers.First;
        // }

        return false;
    }
}