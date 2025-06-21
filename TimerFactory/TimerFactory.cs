namespace TimerFactory;

public interface ITimerFactory
{
    #region [Definitions]
    event Action<string, TimeSpan>? ActionSuccess;
    event Action<string, Exception>? ActionFailure;
    void AddTimer(string name, TimeSpan interval, Action action);
    void RemoveTimer(string name);
    void KillAllTimers();
    bool StopTimer(string name);
    bool StartTimer(string name, TimeSpan interval);
    System.Threading.Timer? GetTimer(string name);
    IEnumerable<string> GetTimerNames();
    TimeSpan GetTimeSpanUntil(DateTime futureTime);
    #endregion
}

public class TimerFactory : ITimerFactory, IDisposable
{
    #region [Members & Events]
    /// <summary>
    /// Can be used for statistics or logging purposes.
    /// </summary>
    public event Action<string, TimeSpan>? ActionSuccess;

    /// <summary>
    /// Fired when an <see cref="System.Action"/> fails."/>
    /// </summary>
    public event Action<string, Exception>? ActionFailure;

    readonly Dictionary<string, System.Threading.Timer> Timers = new Dictionary<string, System.Threading.Timer>();
    #endregion

    #region [Main Methods]
    /// <summary>
    /// Adds a <see cref="System.Threading.Timer"/> with the specified name that will run the given action at the defined interval.
    /// </summary>
    /// <param name="name">Unique name to identify the timer.</param>
    /// <param name="action">The action to perform on each tick.</param>
    /// <param name="interval">The recurring interval for the action.</param>
    /// <exception cref="ArgumentException">Thrown if a <see cref="System.Threading.Timer"/> with the same name already exists.</exception>
    public void AddTimer(string name, TimeSpan interval, Action action)
    {
        // Ensure that a timer with the specified name does not already exist.
        if (Timers.ContainsKey(name))
            throw new ArgumentException($"A timer with the name '{name}' already exists.");

        // Create a TimerCallback that wraps the provided action.
        System.Threading.TimerCallback callback = (state) =>
        {
            try
            {
                action();
                // This event is trivial, but it could be used for stat tracking or logging.
                ActionSuccess?.Invoke(name, interval);
            }
            catch (Exception ex)
            {
                ActionFailure?.Invoke(name, ex);
            }
        };

        // Create the timer. This timer will first trigger after the interval 
        // and then continue to trigger every interval afterwards.
        var timer = new System.Threading.Timer(callback, null, interval, interval);

        // Save the timer in the dictionary.
        Timers.Add(name, timer);
    }

    /// <summary>
    /// Removes (and disposes) the timer identified by the specified name.
    /// </summary>
    /// <param name="name">The unique name of the timer to remove.</param>
    public void RemoveTimer(string name)
    {
        if (Timers.TryGetValue(name, out System.Threading.Timer? timer))
        {
            timer?.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            timer?.Dispose();
            Timers.Remove(name);
        }
    }

    /// <summary>
    /// Releases all resources used by the current instance and stops any active timers.
    /// </summary>
    /// <remarks>Call this method when the instance is no longer needed to ensure proper cleanup of resources. After calling <see cref="Dispose"/>, the instance should not be used further.</remarks>
    public void Dispose() => KillAllTimers();
    #endregion

    #region [Helper Methods]
    /// <summary>
    /// Stops all timers and clears the dictionary.
    /// </summary>
    public void KillAllTimers()
    {
        foreach (var timer in Timers.Values)
        {
            timer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            timer.Dispose();
        }
        Timers.Clear();
    }

    /// <summary>
    /// Stops the <see cref="System.Threading.Timer"/> matching the given <paramref name="name"/>.
    /// </summary>
    /// <returns>true if successful, false otherwise</returns>
    public bool StopTimer(string name)
    {
        if (Timers.TryGetValue(name, out System.Threading.Timer? timer))
            return timer?.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite) ?? false;

        return false;
    }

    /// <summary>
    /// Starts the <see cref="System.Threading.Timer"/> matching the given <paramref name="name"/>.
    /// </summary>
    /// <returns>true if successful, false otherwise</returns>
    public bool StartTimer(string name, TimeSpan interval)
    {
        if (Timers.TryGetValue(name, out System.Threading.Timer? timer))
            return timer?.Change(interval, interval) ?? false;

        return false;
    }

    /// <summary>
    /// Returns a <see cref="System.Threading.Timer"/> matching <paramref name="name"/>, otherwise null.
    /// </summary>
    public System.Threading.Timer? GetTimer(string name)
    {
        if (Timers.TryGetValue(name, out System.Threading.Timer? timer))
            return timer;

        return null;
    }

    /// <summary>
    /// Returns all timer names as an enumerable collection.
    /// </summary>
    /// <returns>A snapshot list of all timer names.</returns>
    public IEnumerable<string> GetTimerNames()
    {
        // Returning as a new List to ensure a snapshot of the keys (prevent potential issues if the underlying dictionary changes)
        return new List<string>(Timers.Keys);
    }

    /// <summary>
    /// Calculates the <see cref="TimeSpan"/> between now and the specified future <see cref="DateTime"/>.
    /// </summary>
    /// <param name="futureTime">The target future time.</param>
    /// <returns>A <see cref="TimeSpan"/> representing the delay, or <see cref="TimeSpan.Zero"/> if the time has already passed.</returns>
    public TimeSpan GetTimeSpanUntil(DateTime futureTime)
    {
        var now = DateTime.UtcNow;
        var delay = futureTime.ToUniversalTime() - now;
        // If time is in the past return TimeSpan.Zero to trigger immediately
        return delay > TimeSpan.Zero ? delay : TimeSpan.Zero;
    }
    #endregion
}
