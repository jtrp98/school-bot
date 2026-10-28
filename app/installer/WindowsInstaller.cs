using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace SchoolBot.Installer;

internal sealed class WindowsInstaller
{
    private volatile bool cancellationRequested;
    private bool cancellationAllowed = true;
    private bool cancellationSent;
    private readonly MsiProgress progress = new();
    private string? error;
    internal void Cancel() => cancellationRequested = true;

    internal InstallResult Install(string package, Action<InstallProgress> report)
    {
        var log = Path.Combine(Path.GetDirectoryName(package)!, "install.log");
        var previousUi = Native.MsiSetInternalUI(2, IntPtr.Zero);
        Native.RecordHandler handler = (_, message, record) => HandleRecord(message, record, report);
        var registered = false;
        try
        {
            var logging = Native.MsiEnableLog(0x3FFF, log, 0);
            if (logging != 0) throw new Win32Exception((int)logging);
            var registration = Native.MsiSetExternalUIRecord(handler, 0x03FFFFFF, IntPtr.Zero, out _);
            if (registration != 0) throw new Win32Exception((int)registration);
            registered = true;
            if (cancellationRequested) return new InstallResult(1602, null, log);
            // Full MSI UI is suppressed. In particular, its completion action must not launch the app a second time.
            var code = Native.MsiInstallProduct(package,
                "REBOOT=ReallySuppress SCHOOLBOT_LAUNCH=0 MSIRESTARTMANAGERCONTROL=Disable");
            return new InstallResult(code, error, log);
        }
        finally
        {
            if (registered) Native.MsiSetExternalUIRecord(null, 0, IntPtr.Zero, out _);
            Native.MsiEnableLog(0, null, 0);
            Native.MsiSetInternalUI(previousUi, IntPtr.Zero);
            GC.KeepAlive(handler);
        }
    }

    internal int HandleRecord(uint message, uint record, Action<InstallProgress> report)
    {
        try
        {
            var kind = (MsiMessage)(message & 0xFF000000);
            switch (kind)
            {
                case MsiMessage.Progress:
                    progress.Update(Field(record, 1), Field(record, 2), Field(record, 3), Field(record, 4));
                    report(new InstallProgress(progress.Percent, progress.IsEstimating));
                    return CancellationResponse();
                case MsiMessage.ActionStart:
                    progress.StartAction();
                    return CancellationResponse();
                case MsiMessage.ActionData:
                    progress.ActionData();
                    report(new InstallProgress(progress.Percent, progress.IsEstimating));
                    return CancellationResponse();
                case MsiMessage.FilesInUse:
                case MsiMessage.RestartManagerFilesInUse:
                    error = SetupResources.Text("filesInUse");
                    return 2;
                case MsiMessage.CommonData:
                    if (Field(record, 1) == 2) cancellationAllowed = Field(record, 2) != 0;
                    return 1;
                case MsiMessage.ResolveSource:
                    return 0;
                case MsiMessage.FatalExit:
                case MsiMessage.Error:
                case MsiMessage.OutOfDiskSpace:
                    error ??= FormatRecord(record);
                    return ErrorResponse(message);
                case MsiMessage.User:
                    return ErrorResponse(message);
                case MsiMessage.Warning:
                    return 0;
                default:
                    return 1;
            }
        }
        catch (Exception exception)
        {
            // Exceptions cannot cross the unmanaged callback boundary; MSI remains responsible for rollback.
            error = exception.Message;
            return -1;
        }
    }

    private static int ErrorResponse(uint message) => (message & 0xF) switch
    {
        1 => 2, // OK / Cancel
        2 => 3, // Abort / Retry / Ignore
        3 or 4 => 7, // Yes / No, with or without Cancel: never approve a risky fallback.
        5 or 6 => 2,
        _ => 1
    };

    private int CancellationResponse()
    {
        if (!cancellationRequested || !cancellationAllowed || cancellationSent) return 1;
        cancellationSent = true;
        return 2;
    }

    private static int Field(uint record, uint field) => Native.MsiRecordGetInteger(record, field);

    private static string FormatRecord(uint record)
    {
        uint size = 0;
        Native.MsiFormatRecord(0, record, null, ref size);
        if (size == 0 || size > 32768) return string.Empty;
        var text = new StringBuilder((int)size + 1);
        size++;
        return Native.MsiFormatRecord(0, record, text, ref size) == 0 ? text.ToString() : string.Empty;
    }

    private static class Native
    {
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate int RecordHandler(IntPtr context, uint message, uint record);
        [DllImport("msi.dll", CharSet = CharSet.Unicode, EntryPoint = "MsiInstallProductW")]
        internal static extern uint MsiInstallProduct(string package, string properties);
        [DllImport("msi.dll")]
        internal static extern uint MsiSetInternalUI(uint level, IntPtr owner);
        [DllImport("msi.dll")]
        internal static extern uint MsiSetExternalUIRecord(RecordHandler? handler, uint filter,
            IntPtr context, out IntPtr previous);
        [DllImport("msi.dll", CharSet = CharSet.Unicode, EntryPoint = "MsiEnableLogW")]
        internal static extern uint MsiEnableLog(uint mode, string? path, uint attributes);
        [DllImport("msi.dll")]
        internal static extern int MsiRecordGetInteger(uint record, uint field);
        [DllImport("msi.dll", CharSet = CharSet.Unicode, EntryPoint = "MsiFormatRecordW")]
        internal static extern uint MsiFormatRecord(uint install, uint record, StringBuilder? text, ref uint size);
    }
}

internal sealed record InstallProgress(double Percent, bool IsEstimating);
internal enum MsiMessage : uint
{
    FatalExit = 0x00000000,
    Error = 0x01000000,
    Warning = 0x02000000,
    User = 0x03000000,
    FilesInUse = 0x05000000,
    ResolveSource = 0x06000000,
    OutOfDiskSpace = 0x07000000,
    ActionStart = 0x08000000,
    ActionData = 0x09000000,
    Progress = 0x0A000000,
    CommonData = 0x0B000000,
    RestartManagerFilesInUse = 0x19000000
}

internal sealed record InstallResult(uint Code, string? Error, string LogPath)
{
    internal bool Succeeded => Code is 0 or 3010;
    internal bool RequiresRestart => Code == 3010;
    internal bool Cancelled => Code == 1602;
}
