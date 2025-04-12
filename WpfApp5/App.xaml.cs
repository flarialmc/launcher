using System;
using System.Threading;
using System.Windows;

namespace Flarial.Launcher;

public partial class App : Application
{
#pragma warning disable IDE0052
    static readonly Mutex Mutex;
#pragma warning restore IDE0052

    static App()
    {
        Mutex = new(default, "54874D29-646C-4536-B6D1-8E05053BE00E", out var value);
        if (!value) Environment.Exit(default);
    }
}