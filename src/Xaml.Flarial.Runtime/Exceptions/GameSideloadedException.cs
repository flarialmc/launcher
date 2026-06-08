using System;

namespace Flarial.Runtime.Exceptions;

sealed class GameSideloadedException : Exception
{
    internal GameSideloadedException() : base("The game is sideloaded, please reinstall it.") { }
}