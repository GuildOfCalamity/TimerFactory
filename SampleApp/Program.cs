using System.Diagnostics;
using System.Xml.Linq;

namespace SampleApp;

public class Program
{
    static TimerFactory.ITimerFactory? _timers = null;

    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        
        Console.WriteLine("🔔 Creating timer factory objects…");
        
        _timers = new TimerFactory.TimerFactory();

        #region [Event Handlers]
        _timers.ActionFailure += (name, ex) =>
        {
            Console.WriteLine($"🚨 '{name}': {ex.Message}");
            //_timers.RemoveTimer(name); // We can choose to remove this timer since it caused a problem.
        };

        _timers.ActionSuccess += (name, interval) =>
        {
            Debug.WriteLine($"[DEBUG] '{name}' executed successfully at {DateTime.Now.TimeStampFormat()} with interval of {interval.ToReadableString()}.");
        };
        #endregion

        // Create a timer that runs every 5 seconds.
        _timers.AddTimer("5SecTimer", TimeSpan.FromSeconds(5), () =>
        {
#if NET6_0_OR_GREATER
            if (Random.Shared.NextDouble() <= 0.111)
                throw new Exception("Randomly generated exception for testing, you can ignore this.");
#else
            if (new Random().NextDouble() <= 0.111)
                throw new Exception("Randomly generated exception for testing, you can ignore this.");
#endif
            Console.WriteLine($"🔔 5 second timer executed at {DateTime.Now.TimeStampFormat()}");
        });

        // Create a timer that runs every 30 seconds.
        _timers.AddTimer("30SecTimer", TimeSpan.FromSeconds(30), () =>
        {
            Console.WriteLine($"🔔 30 second timer executed at {DateTime.Now.TimeStampFormat()}");
        });

        // Create a timer that runs every 60 seconds.
        _timers.AddTimer("60SecTimer", TimeSpan.FromSeconds(60), () =>
        {
            Console.WriteLine($"🔔 60 second timer executed at {DateTime.Now.TimeStampFormat()}");
        });

        // Create a timer that runs every 1 day.
        _timers.AddTimer("1DayTimer", TimeSpan.FromDays(1), () =>
        {
            Console.WriteLine($"🔔 1 day timer executed at {DateTime.Now.TimeStampFormat()}");
        });

        // Using the GetTimeSpanUntil helper method to set a timer for a week from now.
        _timers.AddTimer("1WeekTimer", _timers.GetTimeSpanUntil(DateTime.Now.AddDays(1)), () =>
        {
            Console.WriteLine($"🔔 1 week timer executed at {DateTime.Now.TimeStampFormat()}");
        });

        Debug.WriteLine($"[Current Timer List]");
        foreach (var tname in _timers.GetTimerNames())
        {
            Debug.WriteLine($"  - {tname}");
        }

        Console.WriteLine($"✏️ Press any key to dispose of the factory and close the app.");
        var key = Console.ReadKey(true).Key;
        _timers.Dispose();

        Console.WriteLine("🔔 Timer factory disposed. Exiting…");
        Thread.Sleep(2000);
    }
}
