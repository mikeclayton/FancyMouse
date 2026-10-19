using Microsoft.UI.Dispatching;

namespace FancyMouse.WinUI3.Controls.Helpers;

public static class DispatcherQueueExtensions
{
    /// <summary>
    /// Runs <paramref name="action"/> on <paramref name="dispatcherQueue"/>'s thread, and
    /// completes once it has run - immediately if the caller is already on that thread,
    /// otherwise by queueing it.
    /// </summary>
    public static async Task InvokeAsync(this DispatcherQueue dispatcherQueue, Action action)
    {
        ArgumentNullException.ThrowIfNull(dispatcherQueue);
        ArgumentNullException.ThrowIfNull(action);

        // already on the ui thread - just run it
        if (dispatcherQueue.HasThreadAccess)
        {
            action();
            return;
        }

        // the action might be called from a task that we're awaiting
        // so we need to make sure we use the UI thread
        var tcs = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        if (!dispatcherQueue.TryEnqueue(
            () =>
            {
                try
                {
                    action();
                    tcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            }))
        {
            tcs.SetException(
                new InvalidOperationException("DispatcherQueue is not accepting work items."));
        }

        // wait for the task to complete
        await tcs.Task.ConfigureAwait(false);
    }
}
