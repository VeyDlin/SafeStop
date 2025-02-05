namespace SafeStop;

public class SafeStopManager {
    private readonly ManualResetEventSlim stopSignal = new(false);
    private readonly SemaphoreSlim stopSemaphore = new(0, 1);
    private readonly SemaphoreSlim hashSetSemaphore = new(1, 1);
    private HashSet<long> activeSections = new();
    private long idCounter = 0;

    // Has the stop signal been set
    public bool isStopSet => stopSignal.IsSet;

    // If set, will be thrown instead of waiting forever
    public Exception? enterAfterStopException = null;


    // Asynchronously creates a new critical section
    public async Task<DisposeAction> CriticalAsync() {
        var id = await EnterCriticalAsync().ConfigureAwait(false);
        return new DisposeAction(() => ExitCritical(id));
    }


    // Synchronously creates a new critical section
    public DisposeAction Critical() {
        var id = EnterCritical();
        return new DisposeAction(() => ExitCritical(id));
    }


    // Sets the application stop signal
    public void Stop() {
        stopSignal.Set();
    }


    // Asynchronously waits for all critical sections to complete before stopping the application
    public async Task WaitStopAsync() {
        await hashSetSemaphore.WaitAsync().ConfigureAwait(false);
        try {
            if (stopSignal.IsSet && activeSections.Count == 0) {
                stopSemaphore.Release();
            }
        } finally {
            hashSetSemaphore.Release();
        }

        await stopSemaphore.WaitAsync().ConfigureAwait(false);
    }


    // Synchronously waits for all critical sections to complete before stopping the application
    public void WaitStop() {
        hashSetSemaphore.Wait();
        try {
            if (stopSignal.IsSet && activeSections.Count == 0) {
                stopSemaphore.Release();
            }
        } finally {
            hashSetSemaphore.Release();
        }

        stopSemaphore.Wait();
    }


    // Asynchronously enters a new critical section
    private async Task<long> EnterCriticalAsync() {
        while (stopSignal.IsSet) {
            if (enterAfterStopException is not null) {
                throw enterAfterStopException;
            }
            await Task.Delay(100).ConfigureAwait(false);
        }

        await hashSetSemaphore.WaitAsync().ConfigureAwait(false);
        try {
            var id = idCounter++;
            activeSections.Add(id);
            return id;
        } finally {
            hashSetSemaphore.Release();
        }
    }


    // Synchronously enters a new critical section
    private long EnterCritical() {
        while (stopSignal.IsSet) {
            if (enterAfterStopException is not null) {
                throw enterAfterStopException;
            }
            Thread.Sleep(100);
        }

        hashSetSemaphore.Wait();
        try {
            var id = idCounter++;
            activeSections.Add(id);
            return id;
        } finally {
            hashSetSemaphore.Release();
        }
    }


    // Exits a critical section
    private void ExitCritical(long id) {
        hashSetSemaphore.Wait();
        try {
            activeSections.Remove(id);
            if (stopSignal.IsSet && activeSections.Count == 0) {
                stopSemaphore.Release();
            }
        } finally {
            hashSetSemaphore.Release();
        }
    }
}