using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Flarial.Launcher.ViewModels;

namespace Flarial.Launcher.Views;

public partial class SettingsGeneralView : UserControl
{
    public SettingsGeneralView()
    {
        InitializeComponent();
    }

    async void BrowseCustomDll_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not SettingsGeneralViewModel viewModel) return;

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null) return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select Custom DLL",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("DLL files")
                {
                    Patterns = ["*.dll"],
                    MimeTypes = ["application/x-msdownload"]
                },
                FilePickerFileTypes.All
            ]
        });

        if (files.Count > 0)
            viewModel.CustomDllPath = files[0].Path.LocalPath;
    }
}
