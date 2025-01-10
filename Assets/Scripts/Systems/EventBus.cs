public class EventBus : ISystems
{
    public delegate void Action(object obj);
    private event Action Event;

    public void Invoke(object obj)
    {
        Event?.Invoke(obj);
    }

    public void Subscribe(Action listener)
    {
        Event += listener;
    }

    public void Unsubscribe(Action listener)
    {
        Event -= listener;
    }
}