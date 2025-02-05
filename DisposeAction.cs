namespace SafeStop;


public class DisposeAction : IDisposable {
    private bool disposed = false;
    private readonly Action onDispose;

    internal DisposeAction(Action onDispose) {
        this.onDispose = onDispose ?? throw new ArgumentNullException(nameof(onDispose));
    }

    ~DisposeAction() {
        Dispose(false);
    }

    public void Release() {
        Dispose(true);
    }

    public void Dispose() {
        Dispose(true);
    }

    protected virtual void Dispose(bool disposing) {
        if (disposed) {
            return;
        }
        if (disposing) {
            onDispose();
        }
        disposed = true;
    }
}