using System;

namespace Flarial.Runtime.Exceptions;

sealed class MinecraftUnpackagedException : Exception
{
    internal MinecraftUnpackagedException() : base("Minecraft is unpackaged, please reinstall it.") {}
}