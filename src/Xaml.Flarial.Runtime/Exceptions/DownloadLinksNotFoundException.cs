using System;

namespace Flarial.Runtime.Exceptions;

sealed class DownloadLinksNotFoundException : Exception
{
    internal DownloadLinksNotFoundException () : base("Couldn't resolve download links, check your internet connection.") { }
}