using System;

namespace Consolation.Utilities
{
    public static class WindowUtilities
    {
        public static void ExecuteWithConsole(Action<ConsoleWindow>? executeWithWindow, Action? executeWithConsole,
            ConsoleWindow? window = null)
        {
            if (window is null)
                executeWithConsole?.Invoke();
            else
                executeWithWindow?.Invoke(window);
        }
    }
}