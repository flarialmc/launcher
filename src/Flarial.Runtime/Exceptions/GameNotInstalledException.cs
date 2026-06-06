using System;

namespace Flarial.Runtime.Exceptions;

sealed class GameNotInstalledException : Exception
{
    internal GameNotInstalledException() : base("The game isn't installed, please install it.") { }
}