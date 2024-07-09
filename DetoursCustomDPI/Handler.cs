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

    delegate int GetScaleFactorForDeviceDelegate(int device, ref DEVICE_SCALE_FACTOR pScale);
    delegate int GetScaleFactorForMonitorDelegate(IntPtr hMonitor, ref DEVICE_SCALE_FACTOR pScale);
    delegate int GetScaleFactorForWindowDelegate(IntPtr hwnd, ref DEVICE_SCALE_FACTOR pScale);
    delegate uint GetDpiForSystemDelegate();
    delegate uint GetDpiForWindowDelegate(IntPtr hwnd);

    private static bool s_dpiFunctionHooked;
    private static float s_currentDpi;
    private static void HookDpiFunctions()
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
        }
    }
    public static void OverrideDefaltDpi(float dpi)
    {
        HookDpiFunctions();
        s_currentDpi = dpi;
    }

    private static uint GetDpiForWindowHook(IntPtr hwnd) => (uint)s_currentDpi;
    private static uint GetDpiForSystemHook() => (uint)s_currentDpi;

    private static int GetScaleFactorForWindowHook(nint hwnd, ref DEVICE_SCALE_FACTOR pScale)
    {
        pScale = DpiToScaleFactor(s_currentDpi);
        return 0;
    }

    private static int GetScaleFactorForMonitorHook(nint hMonitor, ref DEVICE_SCALE_FACTOR pScale)
    {
        pScale = DpiToScaleFactor(s_currentDpi);
        return 0;
    }

    private static int GetScaleFactorForDeviceHook(int device, ref DEVICE_SCALE_FACTOR pScale)
    {
        pScale = DpiToScaleFactor(s_currentDpi);
        return 0;
    }
}
