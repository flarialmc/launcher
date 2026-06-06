using System;

namespace Flarial.Runtime.Exceptions;

sealed class GameNotFoundException : Exception
{
    internal GameNotFoundException() : base("Cannot find the game's executable, please repair it.") { }
}