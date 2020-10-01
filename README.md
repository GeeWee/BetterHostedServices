# Better Hosted Services
[![GitHub Actions Status](https://github.com/geewee/BetterHostedServices/workflows/Build/badge.svg?branch=main)](https://github.com/geewee/BetterHostedServices/actions)
[![GitHub Actions Build History](https://buildstats.info/github/chart/geewee/BetterHostedServices?branch=main&includeBuildsFromPullRequest=false)](https://github.com/geewee/BetterHostedServices/actions)

This projects is out to solve some limitations with ASP.NET Core's `IHostedService` and `BackgroundService`.

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
