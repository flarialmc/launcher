using System.Windows;
using System.Windows.Controls;
using Flarial.Launcher.Services.Core;
using Flarial.Launcher.Services.Management.Versions;
using static Windows.ApplicationModel.Store.Preview.InstallControl.AppInstallState;
using static ModernWpf.Controls.Symbol;
using System.Windows.Threading;
using static Flarial.Launcher.Interface.MessageDialogContent;
using Flarial.Launcher.Interface.Controls;

namespace Flarial.Launcher.Interface.Pages;

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

    internal VersionsPage(MainWindowContent content)
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

        Application.Current.MainWindow.Closing += (sender, args) =>
        {
            if (args.Cancel = _request is { })
            {
                content.Content = this;
                content._versionsPageItem.IsSelected = true;
            }
        };

        _control._button.Click += async (_, _) =>
        {
            try
            {
                IsEnabled = false;

                if (!Minecraft.IsInstalled)
                {
                    await MessageDialog.ShowAsync(_notInstalled);
                    return;
                }

                if (!Minecraft.IsPackaged)
                {
                    await MessageDialog.ShowAsync(_unpackagedInstallation);
                    return;
                }

                if (!await MessageDialog.ShowAsync(_versionInstallation))
                    return;

                _control._icon.Symbol = Download;
                _control._icon.Visibility = Visibility.Visible;

                _control._progressBar.Visibility = Visibility.Visible;
                _control._progressBar.IsIndeterminate = true;

                _control._button.Visibility = Visibility.Hidden;

                try
                {
                    var entry = (VersionEntry)((ListBoxItem)_listBox.SelectedItem).Tag;
                    await (_request = await entry.InstallAsync((state, value) => Dispatcher.Invoke(() =>
                    {
                        switch (state)
                        {
                            case Installing: _control._icon.Symbol = Upload; break;
                            case Downloading: _control._icon.Symbol = Download; break;
                        }

                        if (value <= 0)
                        {
                            _control._progressBar.Value = 0;
                            _control._progressBar.IsIndeterminate = true;
                        }

                        if (_control._progressBar.Value != value)
                        {
                            _control._progressBar.Value = value;
                            _control._progressBar.IsIndeterminate = false;
                        }
                    }, DispatcherPriority.Send)));
                }
                finally { _request = null; }
            }
            finally
            {
                _control._progressBar.Value = 0;
                _control._progressBar.IsIndeterminate = false;
                _control._progressBar.Visibility = Visibility.Hidden;

                _control._icon.Symbol = Download;
                _control._icon.Visibility = Visibility.Collapsed;

                _control._button.Visibility = Visibility.Visible;
                IsEnabled = true;
            }
        };
    }
}