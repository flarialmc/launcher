using System;
using System.Threading;
using System.Windows;
using static Windows.ApplicationModel.Package;

static class Program
{
    [STAThread]
    static void Main()
    {
        if (!GameLaunchHelper.HasPackageIdentity) return; if (GameLaunchHelper.Activate()) return;
        using Mutex mutex = new(false, Current.Id.FullName, out var @_); if (!_) return;
        new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown }.Run(new MainWindow());
    }
}