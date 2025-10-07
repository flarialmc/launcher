using System;
using System.Threading.Tasks;

namespace Flarial.Launcher.SDK;

/// <summary>
/// Provides methods to manage Minecraft versions compatible with Flarial Client.
/// </summary>

public sealed partial class Catalog
{
    /// <summary>
    /// Asynchronously gets a catalog of versions.
    /// </summary>

    /// <returns>
    /// A catalog of versions supported by Flarial Client.
    /// </returns>

    public static partial Task<Catalog> GetAsync();

    /// <summary>
    /// Asynchronously resolves a download link for the specified version.
    /// </summary>

    /// <param name="value">
    /// The version to resolve.
    /// </param>

    /// <returns>
    /// The download link for the specified version.
    /// </returns>

    public partial Task<Uri> UriAsync(string value);

    /// <summary>
    /// Asynchronously checks if the installed version of Minecraft Bedrock Edition is compatible with Flarial.
    /// </summary>

    /// <returns>
    /// A boolean value that represents compatibility.
    /// </returns>

    public partial Task<bool> CompatibleAsync();

    /// <summary>
    /// Asynchronously installs frameworks required by Minecraft: Bedrock Edition.
    /// </summary>
    
    public static partial Task FrameworksAsync();

    /// <summary>
    /// Asynchronously starts the installation of a version.
    /// </summary>

    /// <param name="value">
    /// The version to be installed.
    /// </param>

    /// <param name="action">
    /// Callback for installation progress.
    /// </param>

    /// <returns>
    /// An installation request.
    /// </returns>

    public partial Task<Request> InstallAsync(string value, Action<int> action = default);
}