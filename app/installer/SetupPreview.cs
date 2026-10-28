#if SETUP_PREVIEW
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SchoolBot.Installer;

internal static class SetupPreview
{
    internal static bool Run(string[] args)
    {
        if (args.Length != 2 || args[0] != "--verify-and-render") return false;
        SetupResources.VerifyPayload();
        VerifyProgress();
        VerifyNativeRecords();
        var family = (FontFamily)Application.Current.FindResource("AppFont");
        var typeface = family.GetTypefaces().First();
        if (!typeface.TryGetGlyphTypeface(out var glyphs) ||
            !glyphs.CharacterToGlyphMap.ContainsKey('ก') || !glyphs.CharacterToGlyphMap.ContainsKey('A'))
            throw new InvalidDataException("The embedded app font must support Thai and Latin offline.");
        var directory = Path.GetFullPath(args[1]);
        Directory.CreateDirectory(directory);
        foreach (var phase in Enum.GetValues<SetupPhase>())
        {
            var window = new MainWindow();
            window.PreviewPhase(phase);
            Render(window, Path.Combine(directory, $"{phase}.png"), 920, 620);
            Render(window, Path.Combine(directory, $"{phase}-small.png"), 736, 496);
            window.Close();
        }
        var animated = new MainWindow();
        animated.PreviewPhase(SetupPhase.Installing);
        for (var frame = 0; frame < 30; frame++)
        {
            animated.PreviewMotion(TimeSpan.FromSeconds(frame / 6.0));
            Render(animated, Path.Combine(directory, $"motion-{frame:D2}.png"), 920, 620);
        }
        animated.Close();
        Assert(!File.ReadAllBytes(Path.Combine(directory, "motion-00.png"))
            .SequenceEqual(File.ReadAllBytes(Path.Combine(directory, "motion-10.png"))), "Animation frames must change");
        File.WriteAllText(Path.Combine(directory, "checks.txt"),
            "PASS: embedded MSI SHA256; offline Thai/Latin font; forward/reverse/reset/action/addition progress; " +
            "native MSI progress/source/files-in-use/cancellation callbacks; cancel/error/restart result handling; " +
            "seven UI states at two sizes; 30 animation frames. " +
            "No MSI action or app launch performed.");
        return true;
    }

    private static void Render(MainWindow window, string path, double width, double height)
    {
        var content = (FrameworkElement)window.Content;
        window.Content = null;
        content.Measure(new Size(width, height));
        content.Arrange(new Rect(0, 0, width, height));
        content.UpdateLayout();
        var bitmap = new RenderTargetBitmap((int)width, (int)height, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(content);
        var pixel = new byte[4];
        bitmap.CopyPixels(new Int32Rect((int)width / 2, (int)height / 2, 1, 1), pixel, 4, 0);
        Assert(pixel[3] > 0, "Preview must contain a visible window");
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var output = File.Create(path);
        encoder.Save(output);
        window.Content = content;
    }

    private static void VerifyProgress()
    {
        var progress = new MsiProgress();
        Assert(progress.Percent == 0, "Unknown total");
        progress.Update(0, 100, 0, 1);
        Assert(progress.IsEstimating, "Estimate phase");
        progress.Update(2, 30, 0, 0);
        Assert(progress.Percent == 30, "Forward progress");
        progress.Update(3, 100, 0, 0);
        Assert(progress.Percent == 15, "Additional ticks");
        progress.Update(1, 10, 1, 0);
        progress.ActionData();
        Assert(progress.Percent == 20, "Action data step");
        progress.StartAction();
        progress.ActionData();
        Assert(progress.Percent == 20, "Action step reset");
        progress.Update(0, 100, 1, 0);
        progress.Update(2, 35, 0, 0);
        Assert(progress.Percent == 65 && !progress.IsEstimating, "Reverse progress");
        progress.Update(2, 1000, 0, 0);
        Assert(progress.Percent == 0, "Progress bounds");
        Assert(new InstallResult(0, null, "").Succeeded, "Success");
        Assert(new InstallResult(3010, null, "").RequiresRestart, "Restart needed");
        Assert(new InstallResult(1602, null, "").Cancelled, "Cancellation");
        Assert(!new InstallResult(1603, null, "").Succeeded, "Failure is never success");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidDataException(message);
    }

    private static void VerifyNativeRecords()
    {
        var record = Native.MsiCreateRecord(4);
        Assert(record != 0, "Native MSI record allocation");
        try
        {
            var installer = new WindowsInstaller();
            double percent = -1;
            void Report(InstallProgress progress) => percent = progress.Percent;
            Native.MsiRecordSetInteger(record, 1, 0);
            Native.MsiRecordSetInteger(record, 2, 200);
            Native.MsiRecordSetInteger(record, 3, 0);
            Native.MsiRecordSetInteger(record, 4, 0);
            Assert(installer.HandleRecord(0x0A000000, record, Report) == 1 && percent == 0,
                "INSTALLMESSAGE_PROGRESS reset reaches progress handler");
            Native.MsiRecordSetInteger(record, 1, 2);
            Native.MsiRecordSetInteger(record, 2, 50);
            installer.HandleRecord(0x0A000000, record, Report);
            Assert(percent == 25, "Native progress is read from integer record fields");
            Assert(installer.HandleRecord(0x06000000, record, Report) == 0, "Source resolution is delegated to MSI");
            Native.MsiRecordSetInteger(record, 1, 2);
            Native.MsiRecordSetInteger(record, 2, 0);
            installer.HandleRecord(0x0B000000, record, Report);
            installer.Cancel();
            Assert(installer.HandleRecord(0x08000000, record, Report) == 1, "Cancellation waits for MSI to allow it");
            Native.MsiRecordSetInteger(record, 2, 1);
            installer.HandleRecord(0x0B000000, record, Report);
            Assert(installer.HandleRecord(0x08000000, record, Report) == 2, "Cancellation is delivered to MSI");
            Assert(installer.HandleRecord(0x08000000, record, Report) == 1, "Rollback is not cancelled a second time");
            Assert(installer.HandleRecord(0x05000000, record, Report) == 2, "Files in use abort without closing user processes");
        }
        finally { Native.MsiCloseHandle(record); }
    }

    private static class Native
    {
        [DllImport("msi.dll")]
        internal static extern uint MsiCreateRecord(uint count);
        [DllImport("msi.dll")]
        internal static extern uint MsiRecordSetInteger(uint record, uint field, int number);
        [DllImport("msi.dll")]
        internal static extern uint MsiCloseHandle(uint handle);
    }
}
#endif
