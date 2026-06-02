using System;
using System.Runtime.ExceptionServices;

namespace Flarial.Launcher;

sealed class ExceptionHandler : IObserver<Exception>
{
    public void OnCompleted() => throw new NotImplementedException();
    public void OnNext(Exception exception) => ExceptionDispatchInfo.Throw(exception);
    public void OnError(Exception exception) => ExceptionDispatchInfo.Throw(exception);
}