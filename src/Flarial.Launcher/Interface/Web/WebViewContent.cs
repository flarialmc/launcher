using System;
using System.Windows;
using Flarial.Launcher.Interface.Presentation;
using Flarial.Launcher.Management;
using Flarial.Launcher.Xaml;
using Windows.UI.Xaml.Controls;

namespace Flarial.Launcher.Interface.Web;

sealed class WebViewContent : System.Windows.Controls.Grid
{
    readonly WebView _view;

    internal WebViewContent(AppSettings settings)
    {
        AppContent content = new(settings);
        (~content).PaneDisplayMode = NavigationViewPaneDisplayMode.LeftCompact;

        XamlHost host = new(~content)
        {
            Width = 960,
            Height = 540
        };

        _view = new()
        {
            Width = 96,
            Height = 540
        };

        ColumnDefinitions.Add(new());
        ColumnDefinitions.Add(new() { Width = GridLength.Auto });

        SetColumn(host, 0);
        SetColumn(_view, 1);

        Children.Add(host);
        Children.Add(_view);

        Loaded += OnLoaded;
    }

    async void OnLoaded(object sender, EventArgs args)
    {
        Loaded -= OnLoaded;
        await _view.Task;

        _view.Instance.Navigate(new("https://example.com"));
    }
}