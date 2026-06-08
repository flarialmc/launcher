using System;

namespace Flarial.Runtime.Exceptions;

sealed class LibraryLoadFailureException : Exception
{
    internal LibraryLoadFailureException() : base("The provided library cannot be loaded, please verify its validity.") { }
}