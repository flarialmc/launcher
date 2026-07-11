using System;
using Flarial.Runtime.Core;

namespace Flarial.Runtime.Client;

public abstract class FlarialClient<T> : FlarialClient where T : FlarialClient<T>, new()
{
    public static readonly T _ = new();

    private protected FlarialClient()
    {
        if (_ is null) return;
        throw new InvalidOperationException();
    }
}