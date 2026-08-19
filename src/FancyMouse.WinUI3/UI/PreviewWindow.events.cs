using FancyMouse.Common.Helpers;

using Microsoft.UI.Xaml;

namespace FancyMouse.WinUI3.UI;

public sealed partial class PreviewWindow
{
    /// <summary>
    /// Wires up every event handler this window subscribes to.
    /// Called from <see cref="InitializeWindow"/>.
    /// </summary>
    private void InitializeEvents()
    {
        this.Activated += this.PreviewWindow_Activated;
        this.PreviewPane.NavigateTo += this.PreviewPane_NavigateTo;
        this.PreviewPane.Cancel += this.PreviewPane_Cancel;
    }

    private void PreviewWindow_Activated(object sender, WindowActivatedEventArgs e)
    {
        switch (e.WindowActivationState)
        {
            case WindowActivationState.CodeActivated:
                this.FocusPreviewPane();
                break;
            case WindowActivationState.Deactivated:
                this.HideWindow();
                break;
            default:
                throw new InvalidOperationException();
        }
    }

    /// <summary>
    /// Handles a navigation signal from <see cref="PreviewPane"/> - a screenshot click or the
    /// equivalent keyboard shortcut (see <see cref="PreviewPane.NavigateTo"/>).
    /// </summary>
    private void PreviewPane_NavigateTo(object? sender, NavigateToEventArgs e)
    {
        var logger = this.Logger;

        logger.Info(string.Join(
            '\n',
            "-----------",
            nameof(PreviewWindow.PreviewPane_NavigateTo),
            "-----------",
            $"device   = {e.Device.Hostname}",
            $"location = {e.Location}"));

        MouseHelper.SetCursorPosition(e.Location);
        this.HideWindow();
    }

    private void PreviewPane_Cancel(object? sender, EventArgs e)
    {
        this.HideWindow();
    }
}
