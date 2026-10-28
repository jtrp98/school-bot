using System.Windows;

namespace SchoolBot.Installer;

public partial class App : Application
{
    private Mutex? instance;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
#if SETUP_PREVIEW
        if (SetupPreview.Run(e.Args)) { Shutdown(); return; }
#endif
        if (e.Args.Length != 0) { Shutdown(87); return; }
        instance = new Mutex(true, "Local\\SchoolBotSetup", out var created);
        if (!created) { Shutdown(1618); return; }
        MainWindow = new MainWindow();
        MainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        instance?.Dispose();
        base.OnExit(e);
    }
}
