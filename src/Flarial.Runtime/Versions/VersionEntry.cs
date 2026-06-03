using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Flarial.Runtime.Game;
using Flarial.Runtime.Services;

namespace Flarial.Runtime.Versions;

sealed class VersionEntry
{
    internal VersionItem? _item;

    internal readonly bool _supported;

    internal VersionEntry(bool supported) => _supported = supported;
}