using System;
using System.Windows;
using Flarial.Launcher.Interface.Presentation;
using Flarial.Launcher.Management;
using Flarial.Launcher.Xaml;
using Windows.UI.Xaml.Controls;

namespace Flarial.Launcher.Interface.Web;

sealed class WebViewContent : System.Windows.Controls.Grid
{
    readonly WebViewHost _webViewHost;

    internal WebViewContent(AppSettings settings)
    {
        AppContent content = new(settings);
        (~content).PaneDisplayMode = NavigationViewPaneDisplayMode.LeftCompact;

        XamlHost _xamlHost = new(~content)
        {
            Width = 960,
            Height = 540
        };

        _webViewHost = new()
        {
            Width = 96,
            Height = 540
        };

        ColumnDefinitions.Add(new());
        ColumnDefinitions.Add(new() { Width = GridLength.Auto });

        SetColumn(_xamlHost, 0);
        SetColumn(_webViewHost, 1);

        Children.Add(_xamlHost);
        Children.Add(_webViewHost);

        Loaded += OnLoaded;
    }

    async void OnLoaded(object sender, EventArgs args)
    {
        Loaded -= OnLoaded;
        await _webViewHost.Task;

        _webViewHost.WebView.Navigate(new("https://example.com"));
    }
}