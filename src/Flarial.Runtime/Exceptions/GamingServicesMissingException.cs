using System;
namespace Flarial.Runtime.Exceptions;

sealed class GamingServicesMissingException : Exception
{
    internal GamingServicesMissingException() : base("Gaming Services isn't installed, please install it.") { }
}