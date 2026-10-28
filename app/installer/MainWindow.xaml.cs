using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Microsoft.Win32;

namespace SchoolBot.Installer;

public partial class MainWindow : Window
{
    private readonly WindowsInstaller installer = new();
    private SetupPhase phase;
    private bool finishing;
    private int lastPercent = -1;
    private bool lastEstimating;
    private bool restartRequired;
    private bool mayLaunch;

    public MainWindow()
    {
        InitializeComponent();
        Title = SetupResources.Text("windowTitle");
        EditionLabel.Text = SetupResources.Text("edition");
        HeroTitle.Text = SetupResources.Text("heroTitle");
        HeroDescription.Text = SetupResources.Text("heroDescription");
        Modes.Text = SetupResources.Text("modes");
        Footer.Text = SetupResources.Format("version", "version", SetupResources.Payload.Version);
        LaunchOption.Content = SetupResources.Text("launch");
        AutomationProperties.SetName(CloseButton, SetupResources.Text("close"));
        AutomationProperties.SetName(MinimizeButton, SetupResources.Text("minimize"));
        AutomationProperties.SetName(InstallProgress, SetupResources.Text("installing"));
        Width = Math.Min(Width, SystemParameters.WorkArea.Width);
        Height = Math.Min(Height, SystemParameters.WorkArea.Height);
        ShowPhase(SetupPhase.Welcome);
        var installedPath = InstalledExecutable();
        if (File.Exists(installedPath) &&
            FileVersionInfo.GetVersionInfo(installedPath).ProductVersion == SetupResources.Payload.Version)
            ShowPhase(SetupPhase.AlreadyInstalled);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (SystemParameters.ClientAreaAnimation)
            ((Storyboard)Resources["FloatAnimation"]).Begin(this, true);
    }

    private void SetBusyAnimation(bool enabled)
    {
        var animation = (Storyboard)Resources["InstallingAnimation"];
        if (enabled && SystemParameters.ClientAreaAnimation) animation.Begin(this, true);
        else animation.Remove(this);
        Orbit.Visibility = enabled ? Visibility.Visible : Visibility.Collapsed;
    }

    private async void OnPrimary(object sender, RoutedEventArgs e)
    {
        if (finishing || phase is SetupPhase.Installing or SetupPhase.Cancelling) return;
        if (phase != SetupPhase.Welcome)
        {
            finishing = true;
            if (phase == SetupPhase.Complete && mayLaunch && !restartRequired && LaunchOption.IsChecked == true)
            {
                try
                {
                    var executable = InstalledExecutable();
                    Process.Start(new ProcessStartInfo(executable)
                    {
                        UseShellExecute = true,
                        WorkingDirectory = Path.GetDirectoryName(executable)!
                    });
                }
                catch (Exception exception) when (exception is Win32Exception or IOException)
                {
                    finishing = false;
                    ShowPhase(SetupPhase.Failed);
                    Description.Text = SetupResources.Text("launchFailed");
                    ShowError(exception.Message);
                    return;
                }
            }
            Close();
            return;
        }
        ShowPhase(SetupPhase.Installing);
        try
        {
            var result = await Task.Run(() => installer.Install(SetupResources.CachePackage(), ReportProgress));
            restartRequired = result.RequiresRestart;
            mayLaunch = result.Succeeded;
            ShowPhase(result.Succeeded ? SetupPhase.Complete :
                result.Cancelled && string.IsNullOrEmpty(result.Error) ? SetupPhase.Cancelled : SetupPhase.Failed);
            if (restartRequired)
            {
                Description.Text = SetupResources.Text("restartRequired");
                LaunchOption.Visibility = Visibility.Collapsed;
            }
            if (phase == SetupPhase.Failed)
            {
                if (result.Code == 1638) Description.Text = SetupResources.Text("newerVersion");
                ShowError(SetupResources.Format("errorCode", "errorCode", result.Code.ToString()) +
                    Environment.NewLine + result.Error + Environment.NewLine +
                    SetupResources.Format("logPath", "logPath", result.LogPath));
            }
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or Win32Exception)
        {
            ShowPhase(SetupPhase.Failed);
            ShowError(exception.Message);
        }
    }

    private void ReportProgress(InstallProgress progress)
    {
        var percent = (int)progress.Percent;
        if (percent == lastPercent && progress.IsEstimating == lastEstimating) return;
        lastPercent = percent;
        lastEstimating = progress.IsEstimating;
        Dispatcher.InvokeAsync(() =>
        {
            if (phase != SetupPhase.Installing) return;
            InstallProgress.Value = progress.Percent;
            ProgressLabel.Text = SetupResources.Text(progress.IsEstimating ? "preparing" : "copying");
        });
    }

    private void OnCancel(object sender, RoutedEventArgs e)
    {
        if (phase == SetupPhase.Cancelling) return;
        if (phase == SetupPhase.Installing)
        {
            installer.Cancel();
            ShowPhase(SetupPhase.Cancelling);
            return;
        }
        Close();
    }

    private void OnClosing(object? sender, CancelEventArgs e)
    {
        if (phase is not (SetupPhase.Installing or SetupPhase.Cancelling)) return;
        e.Cancel = true;
        if (phase == SetupPhase.Installing) OnCancel(sender ?? this, new RoutedEventArgs());
    }

    private void OnDrag(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed &&
            e.OriginalSource is not System.Windows.Controls.Primitives.ButtonBase)
            DragMove();
    }

    private void OnMinimize(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    private void ShowPhase(SetupPhase next)
    {
        phase = next;
        var busy = next is SetupPhase.Installing or SetupPhase.Cancelling;
        var complete = next == SetupPhase.Complete;
        Heading.Text = SetupResources.Text(next switch
        {
            SetupPhase.Welcome => "welcomeTitle",
            SetupPhase.Installing => "installingTitle",
            SetupPhase.Cancelling => "cancellingTitle",
            SetupPhase.Complete => "completeTitle",
            SetupPhase.Cancelled => "cancelledTitle",
            SetupPhase.AlreadyInstalled => "alreadyInstalledTitle",
            _ => "failedTitle"
        });
        Description.Text = SetupResources.Text(next switch
        {
            SetupPhase.Welcome => "welcomeDescription",
            SetupPhase.Installing => "installingDescription",
            SetupPhase.Cancelling => "cancellingDescription",
            SetupPhase.Complete => "completeDescription",
            SetupPhase.Cancelled => "cancelledDescription",
            SetupPhase.AlreadyInstalled => "alreadyInstalledDescription",
            _ => "failedDescription"
        });
        Labs.Visibility = next == SetupPhase.Welcome ? Visibility.Visible : Visibility.Collapsed;
        ProgressPanel.Visibility = busy ? Visibility.Visible : Visibility.Collapsed;
        ProgressLabel.Text = SetupResources.Text(next == SetupPhase.Cancelling ? "rollingBack" : "preparing");
        ProgressNote.Text = SetupResources.Text("progressNote");
        LaunchOption.Visibility = complete ? Visibility.Visible : Visibility.Collapsed;
        PrimaryButton.Content = SetupResources.Text(next == SetupPhase.Welcome ? "install" :
            next == SetupPhase.Installing ? "installing" : next == SetupPhase.Cancelling ? "cancelling" : "finish");
        PrimaryButton.IsEnabled = !busy;
        CancelButton.Content = SetupResources.Text("cancel");
        CancelButton.Visibility = next is SetupPhase.Welcome or SetupPhase.Installing ? Visibility.Visible : Visibility.Collapsed;
        CloseButton.IsEnabled = next != SetupPhase.Cancelling;
        SetBusyAnimation(busy);
    }

    private void ShowError(string detail)
    {
        ErrorDetail.Text = detail.Length > 520 ? detail[..520] : detail;
        ErrorDetail.ToolTip = detail;
        ErrorDetail.Visibility = Visibility.Visible;
    }

    private static string InstalledExecutable()
    {
        using var key = Registry.CurrentUser.OpenSubKey("Software\\SchoolBot");
        var directory = key?.GetValue("InstallDir") as string;
        directory ??= Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Programs", "School Bot");
        return Path.Combine(directory, "school-bot.exe");
    }

#if SETUP_PREVIEW
    internal void PreviewPhase(SetupPhase previewPhase)
    {
        ShowPhase(previewPhase);
        if (previewPhase == SetupPhase.Installing) InstallProgress.Value = 58;
    }

    internal void PreviewMotion(TimeSpan elapsed)
    {
        var floating = (Storyboard)Resources["FloatAnimation"];
        floating.Begin(this, true);
        floating.SeekAlignedToLastTick(this, elapsed, TimeSeekOrigin.BeginTime);
        var installing = (Storyboard)Resources["InstallingAnimation"];
        installing.SeekAlignedToLastTick(this, elapsed, TimeSeekOrigin.BeginTime);
    }
#endif
}

internal enum SetupPhase { Welcome, Installing, Cancelling, Complete, Cancelled, Failed, AlreadyInstalled }
