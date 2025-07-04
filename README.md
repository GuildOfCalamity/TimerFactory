## 💻 Timer Factory

![Icon](./AppIcon.png)

## 📝 v1.0.0.0 - June 2025

**Dependencies**

| Assembly | Version |
| ---- | ---- |
| .NET Core | 6.0 |
| .NET Framework | 4.8 |

## 📰 Description
- A library, and sample app, for the **TimerFactory** demonstrating use of task timers.
- This library can run multiple timers, executing actions at specified intervals.
- This project includes compilation outputs for both **.NET Framework 4.8** and **.NET Core 6.0**

## 🎛️ Library Definitions

`event Action<string, TimeSpan>? ActionSuccess;`

`event Action<string, Exception>? ActionFailure;`

`void AddTimer(string name, TimeSpan interval, Action action);`

`void AddTimer<T>(string name, Func<T> func, TimeSpan interval);`

`void AddTimer<T>(string name, Func<T> func, TimeSpan interval, Action<T> resultHandler);`

`Task<T> AddOneShotTimer<T>(string name, Func<T> func, TimeSpan dueTime);`

`void RemoveTimer(string name);`

`void KillAllTimers();`

`bool StopTimer(string name);`

`bool StartTimer(string name, TimeSpan interval);`

`Timer? GetTimer(string name);`

`IEnumerable<string> GetTimerNames();`

`TimeSpan GetTimeSpanUntil(DateTime futureTime);`


```csharp

    /** Samples **/

    ITimerFactory? _timers = new TimerFactory();
    
    // Create a timer that fires every 1 hour.
    _timers.AddTimer("1HourTimer", TimeSpan.FromHours(1), () =>
    {
        Console.WriteLine($"🔔 1 hour timer executed at {DateTime.Now}");
    });

    // Using the GetTimeSpanUntil helper method to create a timer 
    // that fires once per week (starting a week from now).
    _timers.AddTimer("1WeekTimer", _timers.GetTimeSpanUntil(DateTime.Now.AddDays(7)), () =>
    {
        Console.WriteLine($"🔔 1 week timer executed at {DateTime.Now}");
    });

```

## 🎛️ Running SampleApp

- `C:\> SampleApp`

- `PS> .\SampleApp`

## 📷 Screenshot

![Sample](./Screenshot.png)

## 🧾 License/Warranty
* Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish and distribute copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions: The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
* The software is provided "as is", without warranty of any kind, express or implied, including but not limited to the warranties of merchantability, fitness for a particular purpose and noninfringement. In no event shall the author or copyright holder be liable for any claim, damages or other liability, whether in an action of contract, tort or otherwise, arising from, out of or in connection with the software or the use or other dealings in the software.
* Copyright © 2025. All rights reserved.

## 📋 Proofing
* This application was compiled and tested using *VisualStudio* 2022 on *Windows 10/11* versions **22H2**, **21H2**, **21H1**, and **23H2**.

