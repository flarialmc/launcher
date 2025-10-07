using System.Security;

namespace Flarial.Launcher.SDK;

/// <summary>
/// Provides methods to manage Developer Mode on Windows.
/// </summary>

[SuppressUnmanagedCodeSecurity]
public static partial class Developer
{
    /// <summary>
    /// Check whether developer mode is enabled or not.
    /// </summary>

    public static partial bool Enabled { get; }

    /// <summary>
    /// Request developer mode to be enabled.
    /// </summary>
    
    public static partial void Request();
}