using System;

namespace Flarial.Launcher.Services;

[Obsolete(@"Use `MessageDialog` instead to notify the user.", true)]
public interface INotificationService
{
    void Show(string message);
}