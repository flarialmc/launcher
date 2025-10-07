using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace Flarial.Launcher.SDK;

/// <summary>
/// Represents an installation request for a version.
/// </summary>

public sealed partial class Request
{
    /// <summary>
    ///  Gets an awaiter for the installation request.
    /// </summary>

    public partial TaskAwaiter<object> GetAwaiter();

    /// <summary>
    /// Cancels the installation request.
    /// </summary>

    public partial void Cancel();

    /// <summary>
    /// Releases resources held by the installation request.
    /// </summary>

    public partial void Dispose();
}