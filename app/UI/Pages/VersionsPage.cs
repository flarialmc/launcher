using System.Windows;
using System.Windows.Controls;
using Flarial.Launcher.Services.Core;
using Flarial.Launcher.Services.Management.Versions;
using Flarial.Launcher.UI.Controls;
using ModernWpf.Controls;
using Windows.ApplicationModel.Store.Preview.InstallControl;

namespace Flarial.Launcher.UI.Pages;

sealed class VersionsPage : Grid
{
    internal readonly ListBox _listBox = new()
    {
        VerticalAlignment = VerticalAlignment.Stretch,
        HorizontalAlignment = HorizontalAlignment.Stretch,
        Margin = new(0, 0, 0, 12)
    };

    readonly InstallProgressControl _control = new()
    {
        VerticalAlignment = VerticalAlignment.Stretch,
        HorizontalAlignment = HorizontalAlignment.Stretch
    };

    InstallRequest? _request = null;

    internal VersionsPage(VersionCatalog catalog)
    {
        Margin = new(12);

        RowDefinitions.Add(new());
        RowDefinitions.Add(new() { Height = GridLength.Auto });

        SetRow(_listBox, 0);
        SetColumn(_listBox, 0);
        Children.Add(_listBox);

        SetRow(_control, 1);
        SetColumn(_control, 0);
        Children.Add(_control);

        _control._button.Click += async (_, _) =>
        {
            try
            {
                IsEnabled = false;

                if (!Minecraft.IsInstalled)
                {
                    await MessageDialog.ShowAsync(MessageDialogContent._notInstalled);
                    return;
                }

                if (!Minecraft.IsSigned)
                {
                    await MessageDialog.ShowAsync(MessageDialogContent._notSigned);
                    return;
                }

                _control._button.Visibility = Visibility.Hidden;

                _control._progressBar.Visibility = Visibility.Visible;
                _control._progressBar.IsIndeterminate = true;

                _control._icon.Visibility = Visibility.Visible;
                _control._icon.Symbol = Symbol.Refresh;

                try
                {
                    var entry = catalog[(string)_listBox.SelectedItem];

                    _request = await entry.InstallAsync((sender, args) => Dispatcher.Invoke(() =>
                    {
                        if (_control._progressBar.Value == args) return;

                        if (sender is AppInstallState.Downloading) _control._icon.Symbol = Symbol.Download;
                        else if (sender is AppInstallState.Installing) _control._icon.Symbol = Symbol.Save;

                        _control._progressBar.Value = args;
                        _control._progressBar.IsIndeterminate = false;
                    }));

                    if (!await _request) Application.Current.Shutdown();
                }
                finally
                {
                    _request?.Dispose();
                    _request = null;
                }

            }
            finally
            {
                _control._progressBar.IsIndeterminate = false;
                _control._button.Visibility = Visibility.Visible;


                _control._icon.Visibility = Visibility.Collapsed;
                _control._icon.Symbol = Symbol.Refresh;

                _control._progressBar.Visibility = Visibility.Hidden;

                IsEnabled = true;
            }
        };

        Application.Current.MainWindow.Closing += (sender, args) =>
        {
            args.Cancel = _request?.Cancel() ?? false;
            if (args.Cancel) ((Window)sender).Hide();
        };
    }
}