namespace BetterHostedServices
{
    /// <summary>
    ///
    /// </summary>
    public enum PeriodicTaskFailureMode
    {
        /// <summary>
        /// If this failure mode is set and a task throws an uncaught application
        /// the task will crash the application.
        /// It has the same behaviour as letting the task bubble up from a CriticalBackgroundService.
        /// </summary>
        CrashApplication = 1,

        /// <summary>
        /// If this failure mode is set and a task throws an uncaught application, the error will be logged via
        /// the standard ILogger implementation, and nothing further will be done.
        /// The next periodic task will continue as planned.
        /// </summary>
        RetryLater = 5
    }
}
