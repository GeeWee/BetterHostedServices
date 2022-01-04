# Better Hosted Services
[![GitHub Actions Status](https://github.com/geewee/BetterHostedServices/workflows/Build/badge.svg?branch=main)](https://github.com/geewee/BetterHostedServices/actions)
[![GitHub Actions Build History](https://buildstats.info/github/chart/geewee/BetterHostedServices?branch=main&includeBuildsFromPullRequest=false)](https://github.com/geewee/BetterHostedServices/actions)

BetterHostedServices is a tiny library (<500 lines of code not including tests) that aims to improve the experience of running background tasks in ASP.NET Core.
You can read more details about the issues and warts `IHostedService` and `BackgroundService` has [here.](https://www.gustavwengel.dk/difference-and-error-handling-between-hostedservice-and-backgroundservice) 

### Installation
From [nuget](https://www.nuget.org/packages/BetterHostedServices/1.0.0):
```shell
dotnet add package BetterHostedServices
```

And then call `services.AddBetterHostedServices()` inside your `Startup.cs`'s `ConfigureServices`

## BackgroundService, Error handling and CriticalBackgroundService 
Microsoft recommends extending from `BackgroundService` for long running tasks.
However BackgroundServices [fails silently if an uncaught error is thrown](https://www.gustavwengel.dk/difference-and-error-handling-between-hostedservice-and-backgroundservice).

That means this example will not throw an error but simply fail silently.
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
We can do better.

Using `BetterHostedServices` you can inherit from `CriticalBackgroundService` instead of the regular `BackgroundService`.

If an uncaught error happens in a `CriticalBackgroundService` it will be logged, and it will crash the application.

You can use it like this:

```csharp
public class YieldingAndThenCrashingBackgroundService: CriticalBackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield(); // Or some other async work
        throw new Exception("Oh no something really bad happened");
    }
    
    public YieldingAndThenCrashingBackgroundService(IApplicationEnder applicationEnder, ILogger<YieldingAndThenCrashingBackgroundService> logger) : base(applicationEnder, logger) { }
}
```
And then you can use it like any other IHostedService. E.g. inside `ConfigureServices` you add the following:
```csharp
services.AddBetterHostedServices();
services.AddHostedService<YieldingAndThenCrashingCriticalBackgroundService>();
```

That's it! Your CriticalBackgroundService now stops the application if an error happens


### Customizing error handling in CriticalBackgroundService

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
        // Add your custom logging here
        this._applicationEnder.ShutDownApplication(); // or simply call base.OnError
    }

    public YieldingAndThenCrashingCriticalBackgroundService(IApplicationEnder applicationEnder, ILogger<YieldingAndThenCrashingCriticalBackgroundService> logger) : base(applicationEnder, logger)
    {
    }
}
```

## AddHostedServiceAsSingleton
Hosted Services and BackgroundServices aren't part of the dependency injection container. This means that you can't get them injected into your services or controllers. If you need to do this, you can use the `AddHostedServiceAsSingleton` extension method on the `IServiceCollection`

```
services.AddHostedServiceAsSingleton<ISomeBackgroundService, SomeBackgroundService>();
```
After that, you can inject them via the DI container just like any ordinary singleton.

## RunPeriodicTasks
If you simply want your BackgroundService to run a periodic tasks, there's some boilerplate you generally have to deal with.
Some best-practices for using BackgroundServices to run periodic tasks are [documented here](https://www.gustavwengel.dk/testing-and-scope-management-aspnetcore-backgroundservices) - but we provide a shortcut here. 

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
