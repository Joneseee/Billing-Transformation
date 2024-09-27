using System.Runtime.InteropServices;

namespace IntegrationLogic;

public static class ConsoleWindow
{
    private static class NativeFunctions
    {
        public enum StdHandle
        {
            StdInputHandle = -10,
            StdOutputHandle = -11,
            StdErrorHandle = -12,
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetStdHandle(int nStdHandle);

        public enum ConsoleMode : uint
        {
            EnableEchoInput = 0x0004,
            EnableExtendedFlags = 0x0080,
            EnableInsertMode = 0x0020,
            EnableLineInput = 0x0002,
            EnableMouseInput = 0x0010,
            EnableProcessedInput = 0x0001,
            EnableQuickEditMode = 0x0040,
            EnableWindowInput = 0x0008,
            EnableVirtualTerminalInput = 0x0200,

            // Screen buffer handle
            EnableProcessedOutput = 0x0001,
            EnableWrapAtEolOutput = 0x0002,
            EnableVirtualTerminalProcessing = 0x0004,
            DisableNewlineAutoReturn = 0x0008,
            EnableLvbGridWorldwide = 0x0010
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);
    }

    public static void QuickEditMode(bool bEnable)
    {
        // Quick Edit lets the user select text in the console window with the mouse, to copy to the windows clipboard.
        // But selecting text stops the console process. This may not be always wanted.
        IntPtr consoleHandle = NativeFunctions.GetStdHandle((int)NativeFunctions.StdHandle.StdInputHandle);

        NativeFunctions.GetConsoleMode(consoleHandle, out uint consoleMode);
        if (bEnable)
            consoleMode |= (uint)NativeFunctions.ConsoleMode.EnableQuickEditMode;
        else
            consoleMode &= ~(uint)NativeFunctions.ConsoleMode.EnableQuickEditMode;

        consoleMode |= ((uint)NativeFunctions.ConsoleMode.EnableExtendedFlags);

        NativeFunctions.SetConsoleMode(consoleHandle, consoleMode);
    }
}