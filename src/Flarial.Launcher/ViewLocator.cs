using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Flarial.Launcher.ViewModels;
using Flarial.Launcher.Views;

namespace Flarial.Launcher;

/// <summary>
/// Given a view model, returns the corresponding view if possible.
/// </summary>
public class ViewLocator : IDataTemplate
{
    public Control? Build(object? param)
    {
        if (param is null)
            return null;

        if (s_factories.TryGetValue(param.GetType(), out var factory))
        {
            return factory();
        }

        return new TextBlock { Text = "Not Found: " + param.GetType().FullName };
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }

    static readonly IReadOnlyDictionary<Type, Func<Control>> s_factories =
        new Dictionary<Type, Func<Control>>
        {
            [typeof(HomeViewModel)] = static () => new HomeView(),
            [typeof(SettingsViewModel)] = static () => new SettingsView(),
            [typeof(SettingsGeneralViewModel)] = static () => new SettingsGeneralView(),
            [typeof(SettingsVersionsViewModel)] = static () => new SettingsVersionsView(),
            [typeof(SettingsConfigsViewModel)] = static () => new SettingsConfigsView(),
            [typeof(VersionItemViewModel)] = static () => new VersionItemView(),
            [typeof(MessageBoxViewModel)] = static () => new MessageBoxView(),
            [typeof(NotificationAreaViewModel)] = static () => new NotificationAreaView(),
            [typeof(NotificationViewModel)] = static () => new NotificationView()
        };
}
