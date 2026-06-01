using System;

namespace Flarial.Runtime.Exceptions;

sealed class MinecraftNotFoundException : Exception
{
    internal MinecraftNotFoundException() : base("Cannot find Minecraft's executable, verify the game's files.") { }
}