using System;

namespace Flarial.Runtime.Exceptions;

sealed class MinecraftSideloadedException : Exception
{
    internal MinecraftSideloadedException() : base("Minecraft is sideloaded, please reinstall it.") { }
}