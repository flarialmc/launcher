using System;

namespace Flarial.Runtime.Exceptions;

sealed class DownloadUriNotFoundException : Exception
{
    internal DownloadUriNotFoundException () : base("Couldn't resolve download links, please check your internet.") { }
}