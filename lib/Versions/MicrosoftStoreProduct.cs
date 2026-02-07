using System;
using System.Linq;
using System.Threading.Tasks;
using Flarial.Launcher.Services.Game;
using Windows.ApplicationModel.Store.Preview.InstallControl;

sealed class MicrosoftGamingServices : MicrosoftStoreProduct
{
    protected override string ProductId => "9MWPM2CQNLHN";
    internal override string PackageFamilyName => "Microsoft.GamingServices_8wekyb3d8bbwe";
}

public abstract class MicrosoftStoreProduct
{
    public static readonly MicrosoftStoreProduct MicrosoftGamingServices = new MicrosoftGamingServices();

    protected abstract string ProductId { get; }
    internal abstract string PackageFamilyName { get; }

    static readonly AppInstallManager s_appInstallManager = new();

    public bool Installed => Minecraft.s_packageManager.FindPackagesForUser(string.Empty, PackageFamilyName).Any();

    internal async Task InstallAsync(Action<int> action)
    {
        if (Installed)
            return;

        await Task.Run(() =>
        {
            foreach (var item in s_appInstallManager.AppInstallItems)
                if (item.GetCurrentStatus().InstallState is AppInstallState.Error) item.Cancel();
        });

        if (await s_appInstallManager.StartAppInstallAsync(ProductId, string.Empty, false, false) is not { } item)
            return;

        TaskCompletionSource<bool> source = new();

        s_appInstallManager.MoveToFrontOfDownloadQueue(item.ProductId, string.Empty);
        _ = source.Task.ContinueWith(_ => item.Cancel(), TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously);

        item.StatusChanged += (sender, args) =>
        {
            var status = sender.GetCurrentStatus(); action((int)status.PercentComplete);
            s_appInstallManager.MoveToFrontOfDownloadQueue(ProductId, string.Empty);
        };

        item.Completed += (_, _) => source.TrySetResult(true);
        await source.Task;
    }
}