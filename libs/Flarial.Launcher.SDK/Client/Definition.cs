using System;
using System.Threading.Tasks;

namespace Flarial.Launcher.SDK;


/// <summary>
/// Provides methods to interact with Flarial Client's dynamic link library.
/// </summary>

public static partial class Client
{
    /// <summary>
    /// Asynchronously download Flarial Client's dynamic link library.
    /// </summary>

    /// <param name="value">
    /// Specify <c>true</c> to download Flarial Client's Beta.
    /// </param>

    /// <param name="action">
    /// Callback for download progress.
    /// </param>

    public static partial Task DownloadAsync(bool value = false, Action<int> action = null);

    /// <summary>
    /// Asynchronously launch Minecraft &#38; load Flarial Client's dynamic link library.
    /// </summary>

    /// <param name="value">
    /// Specify <c>true</c> to use Flarial Client's Beta.
    /// </param>

    /// <returns>
    /// If the game initialized &amp; launched successfully or not.
    /// </returns>

    public static partial Task<bool> LaunchAsync(bool value = false);

    /// <summary>
    /// Asynchronously launch Minecraft &#38; load Flarial Client's dynamic link library.
    /// </summary>
    /// <param name="wait">Wait for the game to initialize.</param>
    /// <param name="beta">Specify <c>true</c> to use Flarial Client's Beta.</param>
    /// <returns>If the game initialized &amp; launched successfully or not.</returns>

    public static partial Task<bool> ActivateAsync(bool wait, bool beta);
}