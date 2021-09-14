# Better Hosted Services
[![GitHub Actions Status](https://github.com/geewee/BetterHostedServices/workflows/Build/badge.svg?branch=main)](https://github.com/geewee/BetterHostedServices/actions)
[![GitHub Actions Build History](https://buildstats.info/github/chart/geewee/BetterHostedServices?branch=main&includeBuildsFromPullRequest=false)](https://github.com/geewee/BetterHostedServices/actions)

This projects is out to solve some limitations with ASP.NET Core's `IHostedService` and `BackgroundService`.
The project also works with console applications using a [.NET Generic Host](https://docs.microsoft.com/en-us/dotnet/core/extensions/generic-host).

### Problem 1. IHostedService is not good for long running tasks.
Creating an `IHostedService` with a long-running task, will delay application startup.
A class like this will never let the application start.

```csharp
public class MyHostedService: IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            // Do some stuff here
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
```

### Problem 2. BackgroundServices fail silently if an error occurs
Microsoft recommends extending from `BackgroundService` for long running tasks.
However BackgroundServices fails silently if an uncaught error occurs.

This example will not throw an error, but simply fail silently.
```csharp
public class YieldingAndThenCrashingCriticalBackgroundService: BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield(); // Or some other async work
        throw new Exception("Oh no something really bad happened");
    }
}
```
You'll simply never know this error happened. We can do better.

### Problem 3. HostedServices are not part of the default DI container
If you want to interact with a HostedService from a controller or another service, you're out of luck.
They're completely separate. There's no built-in way to get a reference to a running `IHostedService`


# Introducing BetterHostedServices

BetterHostedServices is a tiny library (<200 lines of code not including tests) that solves some of these issues.

## CriticalBackgroundService
It introduces a class you can inherit from: `CriticalBackgroundService` - if an uncaught error happens in a `CriticalBackgroundService`
it will log it and stop the application

You can use it like this:
Inherit from the CriticalBackgroundService

```csharp
public class YieldingAndThenCrashingBackgroundService: CriticalBackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield(); // Or some other async work
        throw new Exception("Oh no something really bad happened");
    }
}
```
Add `AddBetterHostedServices()` and your hosted service inside `Startup.cs`'s `ConfigureServices` method.
```csharp
services.AddBetterHostedServices();
services.AddHostedService<YieldingAndThenCrashingCriticalBackgroundService>();
```

That's it! Your CriticalBackgroundService now stops the application if an error happens


### Customizing error handling

If you need to customize error logging or handle the error in another way, you can override the `OnError` method.


```csharp
public class YieldingAndThenCrashingCriticalBackgroundService : CriticalBackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield(); // Hand over control explicitly, to ensure this behaviour also works
        throw new Exception("Crash after yielding");
    }

    protected override void OnError(Exception exceptionFromExecuteAsync)
    {
        // Custom logging here
        this._applicationEnder.ShutDownApplication(); // or simply call base.OnError
    }

    public YieldingAndThenCrashingCriticalBackgroundService(IApplicationEnder applicationEnder) : base(applicationEnder)
    {
    }
}
```

## AddHostedServiceAsSingleton
Occasionally you might want to interact with a Hosted Service from the 
rest of your application. You can do this via the `AddHostedServiceAsSingleton`
method on the `IServiceCollection`

```
services.AddHostedServiceAsSingleton<ISomeBackgroundService, SomeBackgroundService>();
```
After that, you can inject them via the DI container just like any ordinary singleton.

## RunPeriodicTasks
If you simply want your BackgroundService to run a periodic tasks, there's some boilerplate you generally have to deal with.
Best-practices for using BackgroundServices to run periodic tasks are [documented here](https://www.gustavwengel.dk/testing-and-scope-management-aspnetcore-backgroundservices) - but you can also use this library.

If you want to run a periodic task, implement the `IPeriodicTask` interface, and use the `IServiceCollection.AddPeriodicTask` method, like below.

```csharp
public class MyPeriodicTask: IPeriodicTask
    {
        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Do some businesss logic
        }
    }
```

```charp
# In ConfigureServices
services.AddPeriodicTask<MyPeriodicTask>(failureMode: PeriodicTaskFailureMode.CrashApplication, timeBetweenTasks: TimeSpan.FromSeconds(5));
```
You can determine two different Failure Modes:
- `PeriodicTaskFailureMode.CrashApplication` : If this is set and the task throws an uncaught exception, the exception will bubble up and crash the application. This is similar to how an uncaught exception inside a `CriticalBackgroundService` works. Use this if you do not expect your tasks to fail, and it would be unwise to continue if one did.
- `PeriodicTaskFailureMode.RetryLater` : If you expect that tasks might fail occasionally, and that retrying is safe, use this method. It will log the error and run a new task after `timeBetweenTasks` has elapsed.

The parameter `timeBetweenTasks` is how long until after the completion of the previous task, the next one should start. The clock will not start ticking until the previous task has completed so there is no risk of tasks overlapping in time.
