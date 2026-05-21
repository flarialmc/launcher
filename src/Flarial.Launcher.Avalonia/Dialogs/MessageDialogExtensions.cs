using System.Threading.Tasks;

namespace Flarial.Launcher.Dialogs;

public static class MessageDialogExtensions
{
    extension(MessageDialog)
    {
        public static async Task<bool> ShowAsync<T>() where T : MessageDialog, new()
        {
            return (await MessageDialog.ShowAsync<T>()) > 0;
        }
    }
}