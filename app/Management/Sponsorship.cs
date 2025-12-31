using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static System.Windows.Media.Imaging.BitmapCreateOptions;
using static System.Windows.Media.Imaging.BitmapCacheOption;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;
using Flarial.Launcher.Services.Networking;
using System;

namespace Flarial.Launcher.Management;

static class Sponsorship
{
    const string CampaignUri = "https://litebyte.co/minecraft?utm_source=flarial-client&utm_medium=app&utm_campaign=bedrock-launch";

    const string BannerUri = "https://litebyte.co/images/flarial.png";

    internal static async Task<Image?> GetAsync()
    {
        try
        {
            using MemoryStream stream = new(await HttpService.GetAsync<byte[]>(BannerUri));

            Image image = new()
            {
                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalAlignment = HorizontalAlignment.Center,
                Height = 50,
                Width = 320,
                Margin = new(0, 0, 0, 12),
                Cursor = Cursors.Hand,
                Source = BitmapFrame.Create(stream, PreservePixelFormat, OnLoad)
            };

            image.MouseLeftButtonDown += (_, _) => { try { using (Process.Start(CampaignUri)) { } } catch { } };
            return image;
        }
        catch { return null; }
    }
}