using System;
using Flarial.Launcher.Types;

namespace Flarial.Launcher;

static class AppMessageBus
{
    public static event Action<PageTransitions>? PageTransitionRequested;
    public static event Action<WindowStateArgs>? WindowStateRequested;

    public static void Send(PageTransitions page) => PageTransitionRequested?.Invoke(page);

    public static void Send(WindowStateArgs state) => WindowStateRequested?.Invoke(state);
}
