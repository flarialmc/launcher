using System;
using System.Collections.Generic;
using System.Data;
using Flarial.Launcher.Services.Core;
using Flarial.Launcher.Services.Modding;
using Flarial.Launcher.Services.System;

namespace Flarial.Launcher.Services.Client;

public abstract partial class FlarialClient
{
    protected readonly Injector Injector = Injector.UWP;

    readonly string _name, _path;

    internal FlarialClient(string name, string path)
    {
        _name = name;
        _path = path;
    }
}

partial class FlarialClient
{
    static readonly List<FlarialClient> _clients = [];

    static FlarialClient()
    {
        _clients.Add(Beta = new FlarialClientBeta());
        _clients.Add(Stable = new FlarialClientStable());
    }

    public static readonly FlarialClient Stable, Beta;
}

partial class FlarialClient
{
    internal bool IsInjectable
    {
        get
        {
            foreach (var client in _clients)
            {
                if (ReferenceEquals(this, client)) continue;
                else if (client.IsRunning) return false;
            }
            return true;
        }
    }

    internal bool IsRunning
    {
        get
        {
            using Win32Mutex mutex = new(_name);
            return mutex.Exists;
        }
    }

    public void LaunchClient(bool initialized)
    {
        var minecraft = Injector.Minecraft;

        if (!IsInjectable) minecraft.TerminateGame();
        else if (IsRunning) { minecraft.LaunchGame(initialized); return; }

        using var process = Injector.BootstrapGame(initialized, _path);
        using Win32Mutex mutex = new(_name); mutex.Duplicate(process);
    }
}