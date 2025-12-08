using System;
using System.Threading;
using System.Windows;
using static Windows.ApplicationModel.Package;

static class Program
{
    static Program() => AppDomain.CurrentDomain.ProcessExit += delegate
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
    };

    [STAThread]
    static void Main()
    {
        if (!GameLaunchHelper.HasPackageIdentity) return; if (GameLaunchHelper.Activate()) return;
        using Mutex mutex = new(false, Current.Id.FullName, out var @_); if (!_) return;
        new Application { ShutdownMode = ShutdownMode.OnMainWindowClose }.Run(new MainWindow());
    }
}