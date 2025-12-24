using System;
using System.Runtime.InteropServices;

namespace Catan.DebugGui;

internal static class ConsoleHelper
{
    [DllImport("kernel32.dll")]
    private static extern bool AllocConsole();

    public static void AttachConsole()
    {
        AllocConsole();
        Console.WriteLine("Debug console attached.");
    }
}
