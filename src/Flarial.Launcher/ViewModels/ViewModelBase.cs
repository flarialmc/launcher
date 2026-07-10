using System;
using System.Reactive;
using System.Reactive.Linq;
using Avalonia.Media;
using Flarial.Launcher.Types;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Flarial.Launcher.ViewModels;

public abstract partial class ViewModelBase : ReactiveObject
{
    [Reactive]
    private Uri _pfpSource = new("https://raw.githubusercontent.com/megahendick/Flarial.Laucher.Testing/refs/heads/main/person_24dp_FF2438.png", UriKind.Absolute);

    [Reactive]
    private string _username = "Guest";

    [Reactive]
    private Role _role = new();
    
    // wtf why did i think this was going to work
    // todo: change this to be static instead
    [Reactive] 
    private bool _isAnimating;

    public ReactiveCommand<PageTransitions, Unit> NavigateCommand { get; }

    protected ViewModelBase()
    {
        var canNavigate = this.WhenAnyValue(x => x.IsAnimating).Select(anim => !anim);
        NavigateCommand = ReactiveCommand.Create<PageTransitions>(page => MessageBus.Current.SendMessage(page), canNavigate);
    }
}