using System;
using System.Reactive;
using System.Threading.Tasks;
using Windows.System;
using Avalonia.Media;
using Flarial.Launcher.Types;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Flarial.Launcher.ViewModels;

public class SettingsAccountViewModel : ViewModelBase
{
    public ReactiveCommand<Unit, Unit> LoginCommand { get; }

    public SettingsAccountViewModel()
    {
        LoginCommand = ReactiveCommand.CreateFromTask(Login);
    }

    private Task Login()
    {
        if (Username == "Guest")
        {
            Username = "bari2d";
            PfpSource = new Uri("https://images-ext-1.discordapp.net/external/4pF2LzTIVXyoEkFxIUqjfp1Z9msFn0nIBONUMCIpF6I/%3Fsize%3D4096/https/cdn.discordapp.com/avatars/546211976674803712/cfedecac78770d26c1fff64bb2df31e9.png", UriKind.Absolute);
            Role = new Role { Name = "Flarial+", BackgroundBrush = SolidColorBrush.Parse("#40FF2438"), BorderBrush = SolidColorBrush.Parse("#FFFF2438")};
        }
        else
        {
            Username = "Guest";
            PfpSource = new Uri("https://raw.githubusercontent.com/megahendick/Flarial.Laucher.Testing/refs/heads/main/person_24dp_FF2438.png", UriKind.Absolute);
            Role = new Role();
        }
        
        return Task.CompletedTask;
    }
}