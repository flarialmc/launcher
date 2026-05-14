using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

namespace Flarial.Launcher.Controls;

sealed class BannerAdsView : ContentControl
{
    const string AdsUrl = "https://ecoursefree.com/CalcCourse.html";

    const string PreInjectScriptTemplate = """
        (function () {{
            var slotIndex = {0};
            try {{
                var s = document.createElement('style');
                s.id = '__flarial_pre_style';
                s.textContent = `
                    html, body {{ background: #000 !important; color: transparent !important; margin: 0 !important; padding: 0 !important; overflow: hidden !important; height: 100% !important; width: 100% !important; }}
                `;
                (document.head || document.documentElement).appendChild(s);
            }} catch (_) {{}}

            document.addEventListener('DOMContentLoaded', function () {{
                try {{
                    var nodes = document.querySelectorAll('ins.adsbygoogle');
                    nodes.forEach(function (ins, i) {{
                        if (i === slotIndex) {{
                            ins.setAttribute('data-ad-format', 'vertical');
                            ins.setAttribute('data-full-width-responsive', 'false');
                            ins.style.display = 'inline-block';
                            ins.style.width = '160px';
                            ins.style.height = '600px';
                        }} else {{
                            ins.setAttribute('data-adsbygoogle-status', 'done');
                            ins.style.display = 'none';
                        }}
                    }});
                }} catch (_) {{}}
            }});
        }})();
        """;

    const string PostInjectScriptTemplate = """
        (function () {{
            var slotIndex = {0};
            var s = document.createElement('style');
            s.textContent = `
                body > *:not(.__flarial_ad) {{ display: none !important; }}
                body .__flarial_ad {{ display: flex !important; align-items: center !important; justify-content: center !important; width: 100% !important; height: 100vh !important; padding: 0 !important; margin: 0 !important; background: #000 !important; position: fixed !important; inset: 0 !important; }}
                body .__flarial_ad ins.adsbygoogle, body .__flarial_ad ins.adsbygoogle iframe {{ display: inline-block !important; width: 160px !important; height: 600px !important; max-width: 100% !important; max-height: 100% !important; }}
            `;
            document.head.appendChild(s);

            var nodes = document.querySelectorAll('ins.adsbygoogle');
            var ins = nodes[slotIndex] || nodes[0];
            if (!ins) return;

            var wrap = document.createElement('div');
            wrap.className = '__flarial_ad';
            wrap.appendChild(ins);
            document.body.appendChild(wrap);
        }})();
        """;

    readonly WebView2 _webView = new()
    {
        DefaultBackgroundColor = System.Drawing.Color.Black
    };

    readonly int _slotIndex;

    internal BannerAdsView(int slotIndex)
    {
        _slotIndex = slotIndex;
        Background = Brushes.Black;
        Content = _webView;

        _webView.CoreWebView2InitializationCompleted += OnCoreReady;
        Loaded += OnLoaded;
    }

    async void OnLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnLoaded;
        try { await _webView.EnsureCoreWebView2Async(); }
        catch { }
    }

    async void OnCoreReady(object sender, CoreWebView2InitializationCompletedEventArgs e)
    {
        if (!e.IsSuccess) return;

        var core = _webView.CoreWebView2;
        core.Settings.AreDevToolsEnabled = true;
        core.Settings.AreDefaultContextMenusEnabled = true;
        core.Settings.IsStatusBarEnabled = false;
        core.Settings.IsZoomControlEnabled = false;

        var pre = string.Format(CultureInfo.InvariantCulture, PreInjectScriptTemplate, _slotIndex);
        try { await core.AddScriptToExecuteOnDocumentCreatedAsync(pre); }
        catch { }

        core.NavigationCompleted += OnNavigationCompleted;
        core.Navigate(AdsUrl);
    }

    async void OnNavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        if (!e.IsSuccess) return;
        var script = string.Format(CultureInfo.InvariantCulture, PostInjectScriptTemplate, _slotIndex);
        try { await _webView.CoreWebView2.ExecuteScriptAsync(script); }
        catch { }
    }
}
