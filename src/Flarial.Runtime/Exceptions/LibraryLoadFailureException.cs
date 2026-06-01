using System;

namespace Flarial.Runtime.Exceptions;

sealed class LibraryLoadFailureException : Exception
{
    internal LibraryLoadFailureException() : base("Failed to load a library, please verify the validity of the library.") { }
}