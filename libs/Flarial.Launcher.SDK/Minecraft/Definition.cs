using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;


namespace Flarial.Launcher.SDK;

/// <summary>
/// Provides methods to interact with Minecraft: Bedrock Edition.
/// </summary>

public static partial class Minecraft
{
    /// <summary>
    /// Check if Minecraft: Bedrock Edition is installed.
    /// </summary>

    public static partial bool Installed { get; }

    /// <summary>
    /// Check if Minecraft Bedrock is running.
    /// </summary>

    public static partial bool Running { get; }

    /// <summary>
    /// Configure debug mode for Minecraft: Bedrock Edition.
    /// </summary>

    public static partial bool Debug { set; }

    /// <summary>
    /// Launches Minecraft Bedrock Edition.
    /// </summary>

    /// <returns>
    /// If the game initialized &amp; launched successfully or not.
    /// </returns>

    public static partial bool Launch();

    /// <summary>
    /// Launches &amp; loads a dynamic link library into Minecraft: Bedrock Edition.
    /// </summary>

    /// <param name="path">
    /// The dynamic link library to load.
    /// </param>

    /// <returns>
    /// If the game initialized &amp; launched successfully or not.
    /// </returns>

    public static partial bool Launch(string path);

    /// <summary>
    /// Terminates Minecraft: Bedrock Edition.
    /// </summary>

    public static partial void Terminate();

    /// <summary>
    /// Get Minecraft: Bedrock Edition's version.
    /// </summary>

    public static partial string Version { get; }

    /// <summary>
    /// Get any running processes of Minecraft: Bedrock Edition.
    /// </summary>

    public static partial IEnumerable<Process> Processes { get; }

    /// <summary>
    /// Check if Minecraft: Bedrock Edition is unpackaged.
    /// </summary>

    public static partial bool Unpackaged { get; }

    /// <summary>
    /// Asynchronously launches &amp; loads a dynamic link library into Minecraft: Bedrock Edition.
    /// </summary>

    /// <param name="path">
    /// The dynamic link library to load.
    /// </param>

    /// <returns>
    /// If the game initialized &amp; launched successfully or not.
    /// </returns>

    public static partial Task<bool> LaunchAsync(string path);


    /// <summary>
    /// Check if Minecraft: Bedrock Edition is using the Game Development Kit.
    /// </summary>

    public static partial bool GDK { get; }
}