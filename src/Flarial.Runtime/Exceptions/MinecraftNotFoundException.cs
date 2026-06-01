using System;

namespace Flarial.Runtime.Exceptions;

sealed class MinecraftNotFoundException : Exception
{
    internal MinecraftNotFoundException() : base("Cannot find Minecraft's executable, please verify its install.") { }
}