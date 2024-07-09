using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Linq;

namespace DetoursCustomDPI;

public static class Loader
{
    [DllImport("kernel32.dll")]
    public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

    [DllImport("kernel32.dll")]
    public static extern IntPtr GetProcAddress(IntPtr hModule, IntPtr procName);

    [DllImport("kernel32.dll")]
    public static extern IntPtr GetCurrentThread();

    [DllImport("kernel32.dll", EntryPoint = "LoadLibraryW", CharSet = CharSet.Unicode)]
    public static extern IntPtr LoadLibrary(string lpModuleName);

    [DllImport("kernel32.dll", EntryPoint = "GetModuleHandleW", CharSet = CharSet.Unicode)]
    public static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("DetoursDll.dll")]
    public static extern long DetourAttach(ref IntPtr a, IntPtr b);

    [DllImport("DetoursDll.dll")]
    public static extern long DetourUpdateThread(IntPtr a);

    [DllImport("DetoursDll.dll")]
    public static extern long DetourTransactionBegin();

    [DllImport("DetoursDll.dll")]
    public static extern long DetourTransactionCommit();

    [DllImport("DetoursDll.dll")]
    public static extern bool DetoursPatchIAT(IntPtr hModule, IntPtr import, IntPtr real);

    [DllImport("DetoursNetCLR.dll", CharSet=CharSet.Ansi)]
    public static extern void DetoursCLRSetGetProcAddressCache(IntPtr hModule, string procName, IntPtr real);
}
