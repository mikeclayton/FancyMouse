using System.Text.Json;

using FancyMouse.HotKeys;
using FancyMouse.Settings;

namespace FancyMouse.WinUI3.Internal.Helpers;

internal sealed class ConfigHelper : IDisposable
{
    private readonly NLog.ILogger _logger;

    private readonly HotKeyManager _hotKeyManager;

    private FileSystemWatcher? _appSettingsWatcher;

    private AppSettings? _appSettings;
    private EventHandler<HotKeyEventArgs>? _hotKeyPressed;

    public ConfigHelper(NLog.ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _hotKeyManager = new HotKeyManager();
    }

    public string? AppSettingsPath
    {
        get;
        private set;
    }

    public AppSettings? AppSettings
    {
        get
        {
            if (_appSettings is null)
            {
                this.LoadAppSettings();
            }

            return _appSettings;
        }
    }

    public void SetAppSettingsPath(string appSettingsPath)
    {
        this.AppSettingsPath = appSettingsPath;
    }

    public void SetHotKeyEventHandler(EventHandler<HotKeyEventArgs> eventHandler)
    {
        var evt = _hotKeyPressed;
        if (evt is not null)
        {
            _hotKeyManager.HotKeyPressed -= evt;
        }

        _hotKeyPressed = eventHandler;
        _hotKeyManager.HotKeyPressed += eventHandler;
    }

    public void LoadAppSettings()
    {
        _hotKeyManager.SetHotKey(null);
        _appSettings = AppSettingsReader.ReadFile(this.AppSettingsPath
            ?? throw new InvalidOperationException("AppSettings cannot be null"));
        _hotKeyManager.SetHotKey(_appSettings.Hotkey
            ?? throw new InvalidOperationException($"{nameof(_appSettings.Hotkey)} cannot be null"));
    }

    public void StartAppSettingsWatcher()
    {
        // set up the filesystem watcher
        var path = Path.GetDirectoryName(this.AppSettingsPath) ?? throw new InvalidOperationException();
        var filter = Path.GetFileName(this.AppSettingsPath) ?? throw new InvalidOperationException();
        _appSettingsWatcher = new FileSystemWatcher(path, filter)
        {
            NotifyFilter = NotifyFilters.LastWrite,
            EnableRaisingEvents = true,
        };
        _appSettingsWatcher.Changed += this.OnAppSettingsChanged;
    }

    private void OnAppSettingsChanged(object sender, FileSystemEventArgs e)
    {
        if (e.ChangeType != WatcherChangeTypes.Changed)
        {
            return;
        }

        try
        {
            // the file might not have been released yet by the application that saved it
            // and caused the file system event (e.g. notepad) so we need to do a couple
            // of retries to give it a chance to release the lock so we can load the file contents.
            for (var i = 0; i < 3; i++)
            {
                try
                {
                    this.LoadAppSettings();
                    break;
                }
                catch (IOException ex)
                {
                    _logger.Error(ex, "failed to reload app settings, retrying");
                    Thread.Sleep(250);
                }
            }
        }
        catch (JsonException ex)
        {
            // the saved file isn't valid config - retrying won't fix malformed JSON, so just
            // log it and keep whatever settings were already loaded, rather than let the
            // exception vanish silently into FileSystemWatcher's own event dispatch.
            _logger.Error(ex, "failed to reload app settings - config file contains invalid JSON");
        }
    }

    public void Dispose()
    {
        _appSettingsWatcher?.Dispose();
    }
}
