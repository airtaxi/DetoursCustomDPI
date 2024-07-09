using System;
using System.Runtime.InteropServices;

namespace DetoursCustomDPI;

// Based on https://github.com/ahmed605/CoreWindowCustomDPI

public static class Handler
{
    private enum DEVICE_SCALE_FACTOR
    {
        DEVICE_SCALE_FACTOR_INVALID = 0,
        SCALE_100_PERCENT = 100,
        SCALE_120_PERCENT = 120,
        SCALE_125_PERCENT = 125,
        SCALE_140_PERCENT = 140,
        SCALE_150_PERCENT = 150,
        SCALE_160_PERCENT = 160,
        SCALE_175_PERCENT = 175,
        SCALE_180_PERCENT = 180,
        SCALE_200_PERCENT = 200,
        SCALE_225_PERCENT = 225,
        SCALE_250_PERCENT = 250,
        SCALE_300_PERCENT = 300,
        SCALE_350_PERCENT = 350,
        SCALE_400_PERCENT = 400,
        SCALE_450_PERCENT = 450,
        SCALE_500_PERCENT = 500
    };

    private static DEVICE_SCALE_FACTOR DpiToScaleFactor(float dpi) => (DEVICE_SCALE_FACTOR)(int)(dpi * 100 / 96.0f);

    // Delegate declarations
    delegate int GetScaleFactorForDeviceDelegate(int device, ref DEVICE_SCALE_FACTOR pScale);
    delegate int GetScaleFactorForMonitorDelegate(IntPtr hMonitor, ref DEVICE_SCALE_FACTOR pScale);
    delegate int GetScaleFactorForWindowDelegate(IntPtr hwnd, ref DEVICE_SCALE_FACTOR pScale);
    delegate uint GetDpiForSystemDelegate();
    delegate uint GetDpiForWindowDelegate(IntPtr hwnd);

    delegate bool PostMessageDelegate(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
    delegate IntPtr FindWindowExDelegate(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);
    delegate uint GetWindowThreadProcessIdDelegate(IntPtr hWnd, out uint lpdwProcessId);

    static PostMessageDelegate PostMessage;
    static FindWindowExDelegate FindWindowEx;
    static GetWindowThreadProcessIdDelegate GetWindowThreadProcessId;

    private static bool s_dpiFunctionHooked;
    private static float s_dpiForCurrentThread;
    private static bool s_overrideDpi;
    private static void Hook()
    {
        if (!s_dpiFunctionHooked)
        {
            IntPtr shcore = Loader.GetModuleHandle("SHCore.dll");
            if (shcore == IntPtr.Zero)
                shcore = Loader.LoadLibrary("SHCore.dll");

            IntPtr user32 = Loader.GetModuleHandle("User32.dll");
            if (user32 == IntPtr.Zero)
                user32 = Loader.LoadLibrary("User32.dll");

            var getScaleFactorForDevice = Loader.GetProcAddress(shcore, "GetScaleFactorForDevice");
            var getScaleFactorForMonitor = Loader.GetProcAddress(shcore, "GetScaleFactorForMonitor");
            var getScaleFactorForWindow = Loader.GetProcAddress(shcore, (IntPtr)244);

            var getDpiForSystem = Loader.GetProcAddress(user32, "GetDpiForSystem");
            var getDpiForWindow = Loader.GetProcAddress(user32, "GetDpiForWindow");

            Loader.DetourTransactionBegin();
            Loader.DetourUpdateThread(Loader.GetCurrentThread());

            Loader.DetourAttach(ref getScaleFactorForDevice, Marshal.GetFunctionPointerForDelegate< GetScaleFactorForDeviceDelegate>(GetScaleFactorForDeviceHook));
            Loader.DetourAttach(ref getScaleFactorForMonitor, Marshal.GetFunctionPointerForDelegate<GetScaleFactorForMonitorDelegate>(GetScaleFactorForMonitorHook));
            Loader.DetourAttach(ref getScaleFactorForWindow, Marshal.GetFunctionPointerForDelegate<GetScaleFactorForWindowDelegate>(GetScaleFactorForWindowHook));
            Loader.DetourAttach(ref getDpiForSystem, Marshal.GetFunctionPointerForDelegate<GetDpiForSystemDelegate>(GetDpiForSystemHook));
            Loader.DetourAttach(ref getDpiForWindow, Marshal.GetFunctionPointerForDelegate<GetDpiForWindowDelegate>(GetDpiForWindowHook));

            Loader.DetourTransactionCommit();

            s_dpiFunctionHooked = true;

            PostMessage = Marshal.GetDelegateForFunctionPointer<PostMessageDelegate>(Loader.GetProcAddress(user32, "PostMessageW"));
            FindWindowEx = Marshal.GetDelegateForFunctionPointer<FindWindowExDelegate>(Loader.GetProcAddress(user32, "FindWindowExW"));
            GetWindowThreadProcessId = Marshal.GetDelegateForFunctionPointer<GetWindowThreadProcessIdDelegate>(Loader.GetProcAddress(user32, "GetWindowThreadProcessId"));
        }
    }
    public static void OverrideDefaltDpi(float dpi)
    {
        Hook();
        s_dpiForCurrentThread = dpi;
        s_overrideDpi = true;
    }

    private static uint GetDpiForWindowHook(IntPtr hwnd) => (uint)s_dpiForCurrentThread;
    private static uint GetDpiForSystemHook() => (uint)s_dpiForCurrentThread;

    private static int GetScaleFactorForWindowHook(nint hwnd, ref DEVICE_SCALE_FACTOR pScale)
    {
        pScale = DpiToScaleFactor(s_dpiForCurrentThread);
        return 0;
    }

    private static int GetScaleFactorForMonitorHook(nint hMonitor, ref DEVICE_SCALE_FACTOR pScale)
    {
        pScale = DpiToScaleFactor(s_dpiForCurrentThread);
        return 0;
    }

    private static int GetScaleFactorForDeviceHook(int device, ref DEVICE_SCALE_FACTOR pScale)
    {
        pScale = DpiToScaleFactor(s_dpiForCurrentThread);
        return 0;
    }
}
