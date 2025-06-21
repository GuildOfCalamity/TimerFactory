namespace TimerFactory;

public interface ITimerFactory
{
    #region [Definitions]
    event Action<string, TimeSpan>? ActionSuccess;
    event Action<string, Exception>? ActionFailure;
    void AddTimer(string name, TimeSpan interval, Action action);
    void AddTimer<T>(string name, Func<T> func, TimeSpan interval);
    void AddTimer<T>(string name, Func<T> func, TimeSpan interval, Action<T> resultHandler);
    Task<T> AddOneShotTimer<T>(string name, Func<T> func, TimeSpan dueTime);
    void RemoveTimer(string name);
    void KillAllTimers();
    bool StopTimer(string name);
    bool StartTimer(string name, TimeSpan interval);
    System.Threading.Timer? GetTimer(string name);
    IEnumerable<string> GetTimerNames();
    TimeSpan GetTimeSpanUntil(DateTime futureTime);
    void Dispose();
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
        // Create the Timer. It starts after the 'interval' and continues running at that interval.
        var timer = new System.Threading.Timer(callback, null, interval, interval);
        // Save the timer in the dictionary.
        Timers.Add(name, timer);
    }

    /// <summary>
    /// Adds a timer with the specified name that will run the given function at the defined interval.
    /// The return value of the function is ignored.
    /// </summary>
    /// <typeparam name="T">The type of the return value of the function.</typeparam>
    /// <param name="name">Unique name to identify the timer.</param>
    /// <param name="func">The function to execute on each tick.</param>
    /// <param name="interval">The recurring interval for the function.</param>
    /// <exception cref="ArgumentException">Thrown if a timer with the same name already exists.</exception>
    public void AddTimer<T>(string name, Func<T> func, TimeSpan interval)
    {
        if (Timers.ContainsKey(name))
            throw new ArgumentException($"A timer with the name '{name}' already exists.");

        // Create a TimerCallback that wraps the provided function.
        System.Threading.TimerCallback callback = state =>
        {
            try
            {
                // Execute the function and ignore its returned value.
                _ = func();

                // This event is trivial, but it could be used for stat tracking or logging.
                ActionSuccess?.Invoke(name, interval);
            }
            catch (Exception ex)
            {
                ActionFailure?.Invoke(name, ex);
            }
        };
        // Create the Timer. It starts after the 'interval' and continues running at that interval.
        var timer = new System.Threading.Timer(callback, null, interval, interval);
        // Save the timer in the dictionary.
        Timers.Add(name, timer);
    }

    /// <summary>
    /// Adds a timer with the specified name that will run the given function at the defined interval,
    /// and passes the result of the function to a provided result handler.
    /// </summary>
    /// <typeparam name="T">The type of the value returned by the function.</typeparam>
    /// <param name="name">Unique name to identify the timer.</param>
    /// <param name="func">The function to execute on each tick.</param>
    /// <param name="interval">The recurring interval for executing the function.</param>
    /// <param name="resultHandler">A callback that receives the function's result each time it is executed.</param>
    /// <exception cref="ArgumentException">Thrown if a timer with the same name already exists.</exception>
    public void AddTimer<T>(string name, Func<T> func, TimeSpan interval, Action<T> resultHandler)
    {
        if (Timers.ContainsKey(name))
            throw new ArgumentException($"A timer with the name '{name}' already exists.");

        System.Threading.TimerCallback callback = state =>
        {
            try
            {
                // Execute the function and then invoke the result handler with the computed value.
                T result = func();
                resultHandler(result);

                // This event is trivial, but it could be used for stat tracking or logging.
                ActionSuccess?.Invoke(name, interval);
            }
            catch (Exception ex)
            {
                ActionFailure?.Invoke(name, ex);
            }
        };
        // Create the Timer. It starts after the 'interval' and continues running at that interval.
        var timer = new System.Threading.Timer(callback, null, interval, interval);
        // Save the timer in the dictionary.
        Timers.Add(name, timer);
    }

    /// <summary>
    /// Creates a one-shot timer that runs the given function after the specified dueTime,
    /// and returns a Task&lt;T&gt; that completes with the function's result.
    /// 
    /// This Task-based approach is ideal for one-time operations where you want to await the result.
    /// The timer is set to fire only once.
    /// </summary>
    /// <typeparam name="T">The type of the result returned by the function.</typeparam>
    /// <param name="name">Unique name used to identify the timer (optional for tracking).</param>
    /// <param name="func">The function to execute when the timer fires.</param>
    /// <param name="dueTime">The delay after which the timer should fire.</param>
    /// <returns>A Task&lt;T&gt; that completes with the result of <paramref name="func"/>.</returns>
    public Task<T> AddOneShotTimer<T>(string name, Func<T> func, TimeSpan dueTime)
    {
        var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        System.Threading.Timer? timer = null;

        // Timer is configured to fire only once (period = Timeout.InfiniteTimeSpan).
        timer = new System.Threading.Timer(state =>
        {
            // Dispose the timer immediately to ensure it's only a one-shot.
            timer.Dispose();
            try
            {
                T result = func();
                tcs.TrySetResult(result);
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
        }, null, dueTime, System.Threading.Timeout.InfiniteTimeSpan);

        // Optionally, store the timer in our dictionary for management or cancellation.
        if (!string.IsNullOrEmpty(name))
        {
            Timers.Add(name, timer);
        }

        return tcs.Task;
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
