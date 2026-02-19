using System.Security.Policy;
using System.Windows;
using Flarial.Launcher.Management;
using Flarial.Launcher.Xaml;
using Windows.UI.Xaml.Controls;

namespace Flarial.Launcher.Pages;

sealed class SettingsPage : XamlElement<Grid>
{
    internal SettingsPage(ApplicationSettings settings) : base(new())
    {
    }
}