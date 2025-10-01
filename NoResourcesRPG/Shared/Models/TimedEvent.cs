namespace NoResourcesRPG.Shared.Models;

public class TimedEvent
{
    public string Id;                     // unique identifier
    public int Type;                     // unique identifier
    public int SubType;                     // unique identifier
    public DateTime ExecuteAt;           // when the effect should trigger
    public Func<Task> Action;            // what to do when it triggers
    public bool Repeating;               // if it repeats
    public TimeSpan Interval;            // repeat interval
}

