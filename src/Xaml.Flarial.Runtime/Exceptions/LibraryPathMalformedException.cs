using System;

namespace Flarial.Runtime.Exceptions;

sealed class LibraryPathMalformedException : Exception
{
    internal LibraryPathMalformedException() : base("The provided library path is malformed, please verify its validity.") { }
}