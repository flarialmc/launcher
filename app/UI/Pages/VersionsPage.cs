using System.Windows;
using System.Windows.Controls;
using Flarial.Launcher.Services.Core;
using Flarial.Launcher.Services.Management.Versions;
using Flarial.Launcher.UI.Controls;
using static Windows.ApplicationModel.Store.Preview.InstallControl.AppInstallState;
using static ModernWpf.Controls.Symbol;
using System.Windows.Threading;

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

    internal VersionsPage()
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

                if (!Minecraft.IsPackaged)
                {
                    await MessageDialog.ShowAsync(MessageDialogContent._unpackagedInstallationDetected);
                    return;
                }

                _control._button.Visibility = Visibility.Hidden;

                _control._progressBar.Visibility = Visibility.Visible;
                _control._progressBar.IsIndeterminate = false;

                _control._icon.Visibility = Visibility.Visible;
                _control._icon.Symbol = Download;

                try
                {
                    var item = (ListBoxItem)_listBox.SelectedItem;
                    var entry = (VersionEntry)item.Tag;

                    _request = await entry.InstallAsync((state, value) => Dispatcher.Invoke(() =>
                    {
                        if (_control._progressBar.Value == value) return;

                        switch (state)
                        {
                            case Installing: _control._icon.Symbol = Save; break;
                            case Downloading: _control._icon.Symbol = Download; break;
                        }

                        _control._progressBar.Value = value;
                    }, DispatcherPriority.Send));

                    await _request;
                }
                finally { _request = null; }

            }
            finally
            {
                _control._progressBar.Value = 0;
                _control._progressBar.Visibility = Visibility.Hidden;

                _control._icon.Visibility = Visibility.Collapsed;
                _control._icon.Symbol = Download;

                _control._button.Visibility = Visibility.Visible;
                IsEnabled = true;
            }
        };

        Application.Current.MainWindow.Closing += (sender, args) => args.Cancel = _request?.State is Installing;
    }
}