using System.Reactive;
using System.Reactive.Linq;
using Flarial.Launcher.Types;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Flarial.Launcher.ViewModels;

public abstract partial class ViewModelBase : ReactiveObject
{
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