using System;

namespace Flarial.Runtime.Exceptions;

sealed class GamingServicesNotInstalledException : Exception
{
    internal GamingServicesNotInstalledException() : base("Gaming Services isn't installed, please install it.") { }
}