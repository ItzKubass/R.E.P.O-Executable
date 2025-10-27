using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

class KubassTrainer
{
    const int PROCESS_ALL_ACCESS = 0x1F0FFF;

    [DllImport("kernel32.dll")]
    static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    [DllImport("kernel32.dll")]
    static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

    [DllImport("kernel32.dll")]
    static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesWritten);

    [DllImport("psapi.dll")]
    static extern bool EnumProcessModules(IntPtr hProcess, [Out] IntPtr[] lphModule, int cb, out int lpcbNeeded);

    [DllImport("psapi.dll", CharSet = CharSet.Auto)]
    static extern uint GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, [Out] StringBuilder lpBaseName, [In][MarshalAs(UnmanagedType.U4)] int nSize);

    [DllImport("kernel32.dll")]
    static extern bool CloseHandle(IntPtr hObject);

    [DllImport("shell32.dll")]
    static extern IntPtr ShellExecute(IntPtr hwnd, string lpOperation, string lpFile, string lpParameters, string lpDirectory, int nShowCmd);

    static IntPtr GetModuleBaseAddress(IntPtr hProcess, string moduleName)
    {
        IntPtr[] hMods = new IntPtr[1024];
        EnumProcessModules(hProcess, hMods, hMods.Length * IntPtr.Size, out int cbNeeded);
        for (int i = 0; i < cbNeeded / IntPtr.Size; i++)
        {
            StringBuilder modName = new StringBuilder(256);
            GetModuleFileNameEx(hProcess, hMods[i], modName, modName.Capacity);
            if (modName.ToString().ToLower().Contains(moduleName.ToLower()))
                return hMods[i];
        }
        return IntPtr.Zero;
    }

    static ulong ReadPointerChain(IntPtr hProcess, ulong address, ulong[] offsets)
    {
        byte[] buffer = new byte[8];
        int bytesRead;
        for (int i = 0; i < offsets.Length; i++)
        {
            ReadProcessMemory(hProcess, (IntPtr)address, buffer, buffer.Length, out bytesRead);
            address = BitConverter.ToUInt64(buffer, 0) + offsets[i];
        }
        return address;
    }

    static void OpenGitHub()
    {
        try
        {
            ShellExecute(IntPtr.Zero, "open", "https://github.com/ItzKubass", null, null, 1);
        }
        catch (Exception ex)
        {
            // Potichu selžeme - uživatel si toho ani nevšimne
        }
    }

    static void Main()
    {
        Console.Title = "ItzKubass R.E.P.O. v1.0";
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔══════════════════════════════╗");
        Console.WriteLine("║     Created by ItzKubass     ║");
        Console.WriteLine("╚══════════════════════════════╝");
        Console.ResetColor();

        string targetProcess = "REPO";
        Process proc = Process.GetProcessesByName(targetProcess).FirstOrDefault();

        if (proc == null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[ERROR] Proces REPO.exe nebyl nalezen.");
            Console.ResetColor();
            Console.WriteLine("Stiskni libovolnou klávesu pro ukončení...");
            Console.ReadKey();
            return;
        }

        IntPtr hProcess = OpenProcess(PROCESS_ALL_ACCESS, false, proc.Id);
        if (hProcess == IntPtr.Zero)
        {
            Console.WriteLine("[ERROR] Nelze otevřít proces.");
            Console.WriteLine("Stiskni libovolnou klávesu pro ukončení...");
            Console.ReadKey();
            return;
        }

        IntPtr unityBase = GetModuleBaseAddress(hProcess, "UnityPlayer.dll");
        if (unityBase == IntPtr.Zero)
        {
            Console.WriteLine("[ERROR] UnityPlayer.dll nenalezen.");
            CloseHandle(hProcess);
            Console.WriteLine("Stiskni libovolnou klávesu pro ukončení...");
            Console.ReadKey();
            return;
        }

        ulong staminaBase = (ulong)unityBase + 0x01D01310;
        ulong[] staminaOffsets = { 0x0, 0x140, 0x144, 0x0, 0xF0, 0x38, 0x1A0 };
        ulong[] healthOffsets = { 0x0, 0x140, 0x144, 0x0, 0x68, 0xB4 };

        ulong staminaAddress = ReadPointerChain(hProcess, staminaBase, staminaOffsets);
        ulong healthAddress = ReadPointerChain(hProcess, staminaBase, healthOffsets);

        bool staminaEnabled = false;
        bool healthEnabled = false;

        Thread cheatThread = new Thread(() =>
        {
            byte[] staminaVal = BitConverter.GetBytes(40.0f);
            byte[] healthVal = BitConverter.GetBytes(100L);
            int written;

            while (true)
            {
                if (staminaEnabled)
                    WriteProcessMemory(hProcess, (IntPtr)staminaAddress, staminaVal, staminaVal.Length, out written);
                if (healthEnabled)
                    WriteProcessMemory(hProcess, (IntPtr)healthAddress, healthVal, healthVal.Length, out written);

                Thread.Sleep(50);
            }
        });

        cheatThread.IsBackground = true;
        cheatThread.Start();

        while (true)
        {
            Console.WriteLine("\n============ OVLÁDÁNÍ ============");
            Console.WriteLine("[1] Přepnout Infinity Stamina");
            Console.WriteLine("[2] Přepnout Infinity Health");
            Console.WriteLine("[3] Otevřít GitHub (ItzKubass)");
            Console.WriteLine("[4] Ukončit program");
            Console.WriteLine("\nStatus:");

            Console.Write("Stamina: ");
            Console.ForegroundColor = staminaEnabled ? ConsoleColor.Green : ConsoleColor.Red;
            Console.WriteLine(staminaEnabled ? "ZAPNUTO" : "VYPNUTO");
            Console.ResetColor();

            Console.Write("Health:  ");
            Console.ForegroundColor = healthEnabled ? ConsoleColor.Green : ConsoleColor.Red;
            Console.WriteLine(healthEnabled ? "ZAPNUTO" : "VYPNUTO");
            Console.ResetColor();

            Console.Write("\nZadej volbu [1-4]: ");
            string input = Console.ReadLine();

            switch (input)
            {
                case "1":
                    staminaEnabled = !staminaEnabled;
                    break;
                case "2":
                    healthEnabled = !healthEnabled;
                    break;
                case "3":
                    OpenGitHub();
                    break;
                case "4":
                    CloseHandle(hProcess);
                    return;
                default:
                    // Pro neplatnou volbu nic neděláme
                    break;
            }

            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔══════════════════════════════╗");
            Console.WriteLine("║     Created by ItzKubass     ║");
            Console.WriteLine("╚══════════════════════════════╝");
            Console.ResetColor();
        }
    }
}