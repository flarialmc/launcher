using System.Collections.Generic;
using System.Threading.Tasks;

namespace Flarial.Launcher.Services;

public interface IDialogService
{
    Task<string> ShowMessageBoxAsync(
        string title,
        string message,
        IEnumerable<string> buttons);
}