using System;

namespace Flarial.Runtime.Exceptions;

sealed class MinecraftNotInstalledException : Exception
{
    internal MinecraftNotInstalledException() : base("Minecraft isn't installed, please install it.") { }
}