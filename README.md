# SafeStop

> A class for managing the safe shutdown of an application

Allows the creation of critical sections that cannot be interrupted by the application stop signal.

## API

### Critical Sections
- Enter a critical section using the `CriticalAsync()` or `Critical()` methods.
- These methods return a `DisposeAction`. To release the critical section, simply use the `using` statement or call the `Release()` method.

### Stopping
- To request a stop, simply call `Stop()`. This action cannot be undone.
- After calling `Stop()`, use `WaitStopAsync()` or `WaitStop()` to wait for all critical sections to complete.

## Usage Example
```csharp
var safeStop = new SafeStopManager() {
    // If set, an exception will be thrown when entering critical sections after the stop signal is set.
    // Otherwise, it will wait indefinitely.
    enterAfterStopException = new InvalidOperationException("Cannot enter critical section, stop signal is set")
};

// Handling the Ctrl+C signal
Console.CancelKeyPress += (sender, eventArgs) => {
    eventArgs.Cancel = true;
    safeStop.Stop();
    Console.WriteLine("Stop signal received. Shutting down...");
};

// Running a background task
_ = Task.Run(async () => {
    try {
        // The section will automatically be released when using the 'using' statement
        using var critical = await safeStop.CriticalAsync();
        Console.WriteLine("Executing critical task in the background...");
        await Task.Delay(5000); // Simulate work
        Console.WriteLine("Background task completed.");
        critical.Release(); // Manual release of the critical section is also possible
    } catch (Exception ex) {
        Console.WriteLine($"Error: {ex.Message}");
    }
});

// Waiting for all critical sections to complete before shutting down
await safeStop.WaitStopAsync();
Console.WriteLine("All critical tasks completed. The application can be safely stopped.");