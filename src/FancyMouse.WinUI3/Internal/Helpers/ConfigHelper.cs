using FancyMouse.HotKeys;
using FancyMouse.Settings;

namespace FancyMouse.WinUI3.Internal.Helpers;

internal static class ConfigHelper
{
    private static readonly HotKeyManager _hotKeyManager;

    private static FileSystemWatcher? _appSettingsWatcher;

    private static AppSettings? _appSettings;
    private static EventHandler<HotKeyEventArgs>? _hotKeyPressed;

    static ConfigHelper()
    {
        ConfigHelper._hotKeyManager = new HotKeyManager();
    }

    public static string? AppSettingsPath
    {
        get;
        private set;
    }

    public static AppSettings? AppSettings
    {
        get
        {
            if (_appSettings is null)
            {
                ConfigHelper.LoadAppSettings();
            }

            return _appSettings;
        }
    }

    public static void SetAppSettingsPath(string appSettingsPath)
    {
        ConfigHelper.AppSettingsPath = appSettingsPath;
    }

    public static void SetHotKeyEventHandler(EventHandler<HotKeyEventArgs> eventHandler)
    {
        var evt = _hotKeyPressed;
        if (evt is not null)
        {
            _hotKeyManager.HotKeyPressed -= evt;
        }

        _hotKeyPressed = eventHandler;
        _hotKeyManager.HotKeyPressed += eventHandler;
    }

    public static void LoadAppSettings()
    {
        _hotKeyManager.SetHoKey(null);
        _appSettings = AppSettingsReader.ReadFile(ConfigHelper.AppSettingsPath
            ?? throw new InvalidOperationException("AppSettings cannot be null"));
        _hotKeyManager.SetHoKey(_appSettings.Hotkey
            ?? throw new InvalidOperationException($"{nameof(_appSettings.Hotkey)} cannot be null"));
    }

    public static void StartAppSettingsWatcher()
    {
        // set up the filesystem watcher
        var path = Path.GetDirectoryName(ConfigHelper.AppSettingsPath) ?? throw new InvalidOperationException();
        var filter = Path.GetFileName(ConfigHelper.AppSettingsPath) ?? throw new InvalidOperationException();
        _appSettingsWatcher = new FileSystemWatcher(path, filter)
        {
            NotifyFilter = NotifyFilters.LastWrite,
            EnableRaisingEvents = true,
        };
        _appSettingsWatcher.Changed += ConfigHelper.OnAppSettingsChanged;
    }

    private static void OnAppSettingsChanged(object sender, FileSystemEventArgs e)
    {
        if (e.ChangeType != WatcherChangeTypes.Changed)
        {
            return;
        }

        // the file might not have been released yet by the application that saved it
        // and caused the file system event (e.g. notepad) so we need to do a couple
        // of retries to give it a chance to release the lock so we can load the file contents.
        for (var i = 0; i < 3; i++)
        {
            try
            {
                ConfigHelper.LoadAppSettings();
                break;
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex.ToString());
                Thread.Sleep(250);
            }
        }
    }
}
