using FancyMouse.Displays.DeviceManagement;
using System.Diagnostics;
using System.Management;

namespace FancyMouse.Displays;

public sealed class VirtualDisplayManager
{

    #region Constructors

    public VirtualDisplayManager(string rootFolder, string hardwareId)
    {
        this.RootFolder = rootFolder ?? throw new ArgumentNullException(nameof(rootFolder));
        this.HardwareId = hardwareId ?? throw new ArgumentNullException(nameof(hardwareId));
    }

    #endregion

    #region Properties

    public string RootFolder
    {
        get;
    }

    public string HardwareId
    {
        get;
    }

    #endregion

    #region Display Driver

    /// <summary>
    /// Checks if the Amyuni Technologies USB Mobile Monitor Virtual Display driver is installed.
    /// </summary>
    public Dictionary<string, object>? GetDisplayDriver()
    {
        var drivers = VirtualDisplayManager.GetManagementObjects(
            string.Join(" ",
                $"SELECT * FROM Win32_PnPSignedDriver",
                $"WHERE ClassGuid='{DeviceClassGuids.Display}'",
                $"AND Manufacturer='Amyuni'",
                $"AND DeviceName='USB Mobile Monitor Virtual Display'",
                $"AND HardWareID='{this.HardwareId}'"
            )
        ).ToList();
        return drivers.SingleOrDefault();
    }

    /// <summary>
    /// </summary>
    /// <returns></returns>
    public void InstallDisplayDriver()
    {
        this.ExecuteDeviceInstallerCommand(
            new List<string>
            {
                "install",
                Path.Combine(this.RootFolder, "usbmmidd.inf"),
                this.HardwareId
            }
        );
    }

    /// <summary>
    /// </summary>
    /// <returns></returns>
    public void UninstallDisplayDriver()
    {
        this.ExecuteDeviceInstallerCommand(
            new List<string> { "stop", this.HardwareId }
        );
        this.ExecuteDeviceInstallerCommand(
            new List<string> { "remove", this.HardwareId }
        );
    }

    #endregion

    #region Display Adapter

    public Dictionary<string, object>? GetDisplayAdapter()
    {
        var devices = VirtualDisplayManager.GetManagementObjects(
            string.Join(" ",
                $"SELECT * FROM Win32_PnPEntity",
                $"WHERE ClassGuid='{DeviceClassGuids.Display}'",
                $"AND Manufacturer='Amyuni'",
                $"AND Name='USB Mobile Monitor Virtual Display'"
                //$"AND HardWareID='{this.HardwareId}'"
            )
        ).Where(
            device => (device["HardwareID"] is string[] hardwareIds) && (hardwareIds?.Contains(this.HardwareId) ?? false)
        ).OrderBy(d => d["Name"])
        .ToList();
        return devices.SingleOrDefault();
    }

    #endregion

    #region Helpers

    private static List<Dictionary<string, object>> GetManagementObjects(string query)
    {
        return new ManagementObjectSearcher(query).Get()
            .Cast<ManagementObject>()
            .Select(
                obj => obj.Properties
                    .Cast<PropertyData>()
                    .OrderBy(p => p.Name)
                    .ToDictionary(
                        p => p.Name,
                        p => p.Value
                    )
            ).ToList();
    }

    public int ExecuteDeviceInstallerCommand(IEnumerable<string> args)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = Path.Combine(this.RootFolder, "deviceinstaller64.exe"),
            UseShellExecute = true,
            Verb = "RunAs"
        };
        foreach(var arg in args)
        {
            startInfo.ArgumentList.Add(arg);
        }
        var process = Process.Start(startInfo) ?? throw new InvalidOperationException();
        process.WaitForExit();
        var exitcode = process.ExitCode;
        if (exitcode != 0)
        {
            throw new InvalidOperationException(
                $"installer failed with exit code {exitcode}"
            );
        }
        return exitcode;
    }

    #endregion

}