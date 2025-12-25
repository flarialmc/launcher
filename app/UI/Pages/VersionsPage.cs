using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Flarial.Launcher.Services.Core;
using Flarial.Launcher.Services.Management.Versions;
using Flarial.Launcher.Services.SDK;
using Flarial.Launcher.UI.Controls;

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
            IsEnabled = false;
            _control._button.Visibility = Visibility.Hidden;
            _control._progressBar.Visibility = Visibility.Visible;
            _control._progressBar.IsIndeterminate = true;

            _request = await catalog[(string)_listBox.SelectedItem].InstallAsync((_) => Dispatcher.Invoke(() =>
            {
                if (_control._progressBar.Value == _) return;
                _control._progressBar.Value = _; _control._progressBar.IsIndeterminate = false;
            }));

            await _request; _request = null;

            _control._progressBar.IsIndeterminate = false;
            _control._button.Visibility = Visibility.Visible;
            _control._progressBar.Visibility = Visibility.Hidden;
            IsEnabled = true;
        };

        Application.Current.MainWindow.Closing += async (sender,args) =>
        {
            args.Cancel = _request is {};
            if (MessageDialog.IsShown) return;
            if (args.Cancel) await MessageDialog.ShowAsync(MessageDialogContent._versionDownloading);
        };
    }
}