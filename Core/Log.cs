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

    // ---------------- BASIC WRITE ----------------

    public static void Write(string message)
    {
        try
        {
            // 🔒 Defensive: never allow null
            message ??= string.Empty;

            // ⏱ Timestamp first
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

            // ✂️ Truncate overly long messages
            if (message.Length > 2000)
                message = message.Substring(0, 2000) + "...";

            // 🧼 Normalize whitespace
            message = message.Trim();

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

            // 👀 Visible in Visual Studio Output (Debug mode)
            Debug.WriteLine(line);
        }
        catch
        {
            // 🚫 Never crash app due to logging
        }
    }

    // ---------------- CATEGORY WRITE ----------------

    public static void WriteCategory(string category, string message)
    {
        try
        {
            // 🔒 Normalize category (safe + consistent)
            string padded = (category ?? string.Empty)
                .Trim()
                .ToUpper()
                .PadRight(7);

            Write($"{padded} | {message}");
        }
        catch
        {
            // 🚫 Never crash app due to logging
        }
    }

    // ---------------- ROTATION ----------------

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