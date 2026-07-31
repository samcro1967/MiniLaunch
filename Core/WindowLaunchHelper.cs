using MiniLaunch.Core;

public static class WindowLaunchHelper
{
    public static void LaunchAndPosition(
        string type,
        Func<HashSet<IntPtr>> getExistingWindows,
        Func<List<IntPtr>> getCurrentWindows,
        Action startProcess,
        AppConfig app,
        int maxAttempts = 30,
        int delayMs = 100,
        Func<IntPtr, bool>? windowFilter = null,
        bool doubleMove = false)
    {
        Log.WriteCategory("LAUNCH", $"{app.Type} | starting");

        var before = getExistingWindows();

        WindowHelpers.DebugBeforeCount(app.Type, before.Count);

        startProcess();

        IntPtr handle = IntPtr.Zero;

        for (int i = 0; i < maxAttempts; i++)
        {
            WindowHelpers.DebugLaunchAttempt(app.Type, i);

            var after = getCurrentWindows();

            foreach (var h in after)
            {
                if (before.Contains(h))
                    continue;

                if (windowFilter != null && !windowFilter(h))
                    continue;

                handle = h;
                break;
            }

            if (handle != IntPtr.Zero)
                break;

            Thread.Sleep(delayMs);
        }

        if (handle == IntPtr.Zero)
        {
            Log.WriteCategory("LAUNCH", $"{app.Type} | fallback triggered");

            var fallback = getCurrentWindows()
                .FirstOrDefault(h => !before.Contains(h));

            if (fallback != IntPtr.Zero)
            {
                handle = fallback;
                Log.WriteCategory("LAUNCH", $"{app.Type} | fallback used");
            }
        }

        if (handle == IntPtr.Zero)
        {
            WindowHelpers.DebugLaunchFailure(app.Type);
            return;
        }

        WindowHelpers.DebugWindow($"{app.Type.ToUpper()} LAUNCH HANDLE", handle);

        // REL → ABS
        var screen = Screen.AllScreens[app.Monitor];

        int finalX = screen.Bounds.Left + app.X;
        int finalY = screen.Bounds.Top + app.Y;

        WindowHelpers.DebugApply(
            app.Type,
            app.Monitor,
            app.X,
            app.Y,
            finalX,
            finalY,
            app.Width,
            app.Height,
            app.Maximized
        );

        WindowHelpers.MoveWindow(
            handle,
            finalX,
            finalY,
            app.Width,
            app.Height,
            app.Maximized
        );

        Thread.Sleep(100);

        if (doubleMove)
        {
            WindowHelpers.MoveWindow(
                handle,
                finalX,
                finalY,
                app.Width,
                app.Height,
                app.Maximized
            );
        }

        WindowHelpers.DebugWindow($"{app.Type.ToUpper()} AFTER MOVE", handle);
    }
}