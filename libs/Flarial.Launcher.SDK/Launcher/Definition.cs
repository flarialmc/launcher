using System;
using System.Security;
using System.Threading.Tasks;

namespace Flarial.Launcher.SDK;

/// <summary>
/// Provides methods to manage Flarial Client's launcher.
/// </summary>

public static partial class Launcher
{
    /// <summary>
    /// Asynchronously check if a launcher update is available. 
    /// </summary>

    public static partial Task<bool> AvailableAsync();

    /// <summary>
    ///  Asynchronously check &amp; if required, update the launcher.
    /// </summary>

    /// <param name="action">
    /// Callback for update progress.
    /// </param>

    /// <returns>
    /// A boolean value that represents the availability of an update.
    /// </returns>

    public static partial Task<bool> UpdateAsync(Action<int> action = default);
}