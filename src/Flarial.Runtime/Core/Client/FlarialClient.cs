using System;

namespace Flarial.Runtime.Core.Client;

public abstract class FlarialClient<T> : FlarialClient where T : FlarialClient<T>, new()
{
    public static T _ { get; }

    static FlarialClient() => _ = new();

    private protected FlarialClient()
    {
        if (_ is null) return;
        throw new InvalidOperationException();
    }
}