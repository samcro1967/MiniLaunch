using System;
using System.Diagnostics;
using System.IO;

namespace MiniLaunch.Core;

public static class Log
{
    private static readonly object _lock = new();

    private static readonly string _folder =
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MiniLaunch");

    private static readonly string _logFile =
        Path.Combine(_folder, "debug.log");

    private static readonly string _prevFile =
        Path.Combine(_folder, "debug.prev.log");

    private const long MaxSizeBytes = 1_048_576; // 1 MB

    private static bool _initialized = false;

    public static void Write(string message)
    {
        try
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

            if (message.Length > 2000)
                message = message.Substring(0, 2000) + "...";

            var line = $"{timestamp} | {message}";

            lock (_lock)
            {
                if (!_initialized)
                {
                    Directory.CreateDirectory(_folder);
                    _initialized = true;
                }

                RotateIfNeeded();

                File.AppendAllText(_logFile, line + Environment.NewLine);
            }

            System.Diagnostics.Trace.WriteLine(line); // still shows in VS
        }
        catch
        {
            // never crash app due to logging
        }
    }

    private static void RotateIfNeeded()
    {
        if (!File.Exists(_logFile))
            return;

        var info = new FileInfo(_logFile);

        if (info.Length < MaxSizeBytes)
            return;

        try
        {
            if (File.Exists(_prevFile))
                File.Delete(_prevFile);

            File.Move(_logFile, _prevFile);
        }
        catch
        {
            // ignore rotation failure
        }
    }
}