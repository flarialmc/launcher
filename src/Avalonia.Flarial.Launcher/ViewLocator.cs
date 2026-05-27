using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Diagnostics;
using Flarial.Launcher.ViewModels;
using Flarial.Launcher.Views;

namespace Flarial.Launcher;

/// <summary>
/// Given a view model, returns the corresponding view if possible.
/// </summary>

public class ViewLocator : IDataTemplate
{
    static readonly Dictionary<Type, Func<Control>> s_views = [];

    internal static Func<Control>? Get(object? value)
    {
        if (value?.GetType() is not { } type)
            return null;

        if (s_views.TryGetValue(type, out var function))
            return function;

        return null;
    }

    internal static void Add<TViewModel, TView>() where TView : Control, new() => s_views.Add(typeof(TViewModel), static () => new TView());

    static ViewLocator()
    {
        Add<MainWindowViewModel, MainWindow>();

        Add<HomeViewModel, HomeView>();
        Add<SettingsViewModel, SettingsView>();

        Add<MessageBoxViewModel, MessageBoxView>();
        Add<NotificationViewModel, NotificationView>();
        Add<NotificationAreaViewModel, NotificationAreaView>();

        Add<VersionItemViewModel, VersionItemView>();
        Add<SettingsConfigsViewModel, SettingsConfigsView>();
        Add<SettingsGeneralViewModel, SettingsGeneralView>();
        Add<SettingsVersionsViewModel, SettingsVersionsView>();
    }

    public Control? Build(object? param) => (Get(param) ?? throw new TypeAccessException())();

    public bool Match(object? data) => data is ViewModelBase;
}