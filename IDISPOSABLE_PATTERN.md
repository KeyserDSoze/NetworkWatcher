# IDisposable Pattern in Visual Studio Extensions

## Why IDisposable is Important

Visual Studio extensions should properly implement `IDisposable` to:

1. **Clean up unmanaged resources** (proxy connections, network sockets)
2. **Unsubscribe from events** (prevent memory leaks)
3. **Release system resources** (registry changes, certificates)
4. **Ensure graceful shutdown** (no orphaned processes or settings)

## The Pattern We Implemented

### Full IDisposable Pattern

```csharp
public class MyControl : UserControl, IDisposable
{
    private bool disposed = false;

    // Public Dispose method
    public new void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    // Protected virtual Dispose method
    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                // Dispose managed resources here
                // - Unsubscribe from events
                // - Stop services
                // - Close connections
            }

            // Free unmanaged resources here (if any)
            
            disposed = true;
        }

        // Call base class Dispose
        base.Dispose();
    }

    // Finalizer (destructor)
    ~MyControl()
    {
        Dispose(false);
    }
}
```

## What We Do in NetworkWatcherToolWindowControl

### 1. Managed Resources Cleanup
```csharp
if (disposing)
{
    // Unsubscribe from events
    this.Unloaded -= NetworkWatcherToolWindowControl_Unloaded;
    Application.Current.Exit -= Application_Exit;

    // Stop proxy and cleanup
    EnsureProxyStopped();
}
```

### 2. Proxy Cleanup
```csharp
private void EnsureProxyStopped()
{
    if (isProxyRunning || proxy != null)
    {
        StopProxy(); // Unsets system proxy + stops server
    }
}
```

### 3. System Proxy Cleanup
```csharp
private void StopProxy()
{
    // Always unset system proxy
    ProxyHelper.UnsetSystemProxy();
    
    // Stop and dispose proxy server
    if (proxy != null)
    {
        proxy.Stop();
        proxy.Dispose();
        proxy = null;
    }
}
```

## Why This Matters for Network Watcher

### Without Proper Disposal:
❌ System proxy stays enabled after VS closes  
❌ Port 8888 remains in use  
❌ Certificate might remain trusted  
❌ Memory leaks from event handlers  
❌ Orphaned proxy process  

### With Proper Disposal:
✅ System proxy automatically disabled  
✅ Port released  
✅ Clean shutdown  
✅ No memory leaks  
✅ Proper resource cleanup  

## Testing Disposal

To test that disposal works correctly:

### Test 1: Normal Close
```
1. Start Visual Studio
2. Open Network Watcher
3. Start Proxy
4. Close Visual Studio
5. Check: Windows proxy settings should be OFF
6. Check: Port 8888 should be free
```

### Test 2: Debug Stop
```
1. Start Visual Studio
2. Open Network Watcher
3. Start Proxy
4. Start debugging (F5)
5. Stop debugging
6. Check: Proxy should still be running
7. Close Visual Studio
8. Check: Windows proxy settings should be OFF
```

### Test 3: Unload Window
```
1. Start Visual Studio
2. Open Network Watcher
3. Start Proxy
4. Close Network Watcher window
5. Check: Proxy should stop
6. Check: Windows proxy settings should be OFF
```

### Test 4: Crash Scenario
```
1. Start Visual Studio
2. Open Network Watcher
3. Start Proxy
4. Kill Visual Studio process (Task Manager)
5. Restart Visual Studio
6. Check: Windows proxy settings (should be OFF ideally)
```

## Visual Studio Extension Best Practices

### 1. ToolWindowPane
```csharp
public class MyToolWindow : ToolWindowPane
{
    // ToolWindowPane already implements IDisposable
    // It will call Dispose on the Content (UserControl)
    
    public MyToolWindow() : base(null)
    {
        this.Content = new MyControl();
    }
}
```

### 2. UserControl in Tool Window
```csharp
public partial class MyControl : UserControl, IDisposable
{
    // Implement full IDisposable pattern
    // Will be called by ToolWindowPane.Dispose()
}
```

### 3. Package (AsyncPackage)
```csharp
public sealed class MyPackage : AsyncPackage
{
    protected override async Task InitializeAsync(...)
    {
        // Initialize resources
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Cleanup resources
        }
        base.Dispose(disposing);
    }
}
```

## Common Pitfalls to Avoid

### ❌ DON'T: Forget to unsubscribe from events
```csharp
// Memory leak!
Application.Current.Exit += MyHandler;
// Missing: Application.Current.Exit -= MyHandler;
```

### ✅ DO: Unsubscribe in Dispose
```csharp
protected virtual void Dispose(bool disposing)
{
    if (disposing)
    {
        Application.Current.Exit -= MyHandler;
    }
}
```

### ❌ DON'T: Call Dispose recursively
```csharp
~MyClass()
{
    Dispose(); // Wrong! Will cause GC issues
}
```

### ✅ DO: Use the pattern correctly
```csharp
~MyClass()
{
    Dispose(false); // Correct
}
```

### ❌ DON'T: Forget to call base.Dispose()
```csharp
public new void Dispose()
{
    // Cleanup
    // Missing: base.Dispose();
}
```

### ✅ DO: Always call base.Dispose()
```csharp
protected virtual void Dispose(bool disposing)
{
    if (!disposed)
    {
        // Cleanup
        disposed = true;
    }
    base.Dispose(); // Important!
}
```

## Additional Resources

- [IDisposable Pattern (Microsoft Docs)](https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-dispose)
- [Visual Studio SDK Extensibility](https://docs.microsoft.com/en-us/visualstudio/extensibility/)
- [Best Practices for VS Extensions](https://docs.microsoft.com/en-us/visualstudio/extensibility/best-practices)

## Summary

✅ Implemented full IDisposable pattern  
✅ Proper cleanup of proxy resources  
✅ Event unsubscription  
✅ Multiple safety nets (Unloaded, Exit, Finalizer)  
✅ Prevents resource leaks  
✅ Follows VS extension best practices  

This ensures that Network Watcher is a well-behaved Visual Studio extension that cleans up properly under all circumstances.
