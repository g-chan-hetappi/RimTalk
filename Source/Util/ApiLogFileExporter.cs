using System;
using System.IO;
using System.Linq;
using System.Text;
using RimTalk.Data;
using RimTalk.UI;
using UnityEngine;
using Verse;
using Logger = RimTalk.Util.Logger;

namespace RimTalk.Util;

/// <summary>
/// Saves the API log CSV next to Player.log, on export and when the game quits.
/// The in-memory ApiHistory is lost on exit, so this keeps a copy for later inspection.
/// </summary>
[StaticConstructorOnStartup]
public static class ApiLogFileExporter
{
    private const string FileName = "RimTalkLog.csv";

    static ApiLogFileExporter()
    {
        Application.quitting += SaveOnQuit;
    }

    public static void Save(string csv)
    {
        try
        {
            string path = Path.Combine(GetLogDirectory(), FileName);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, csv, new UTF8Encoding(true));
            Logger.Message($"API log saved to: {path}");
        }
        catch (Exception ex)
        {
            Logger.Warning($"Failed to save API log file: {ex.Message}");
        }
    }

    private static void SaveOnQuit()
    {
        try
        {
            var logs = ApiHistory.GetAll().ToList();
            // Keep the previous file when nothing was recorded this session.
            if (logs.Count == 0) return;
            Save(UIUtil.BuildLogCsv(logs));
        }
        catch (Exception ex)
        {
            Logger.Warning($"Failed to build API log on quit: {ex.Message}");
        }
    }

    // Player.log's own directory; fall back to the standard Logs folder under the save data path.
    private static string GetLogDirectory()
    {
        string consoleLog = Application.consoleLogPath;
        string dir = string.IsNullOrEmpty(consoleLog) ? null : Path.GetDirectoryName(consoleLog);
        return !string.IsNullOrEmpty(dir) ? dir : Path.Combine(GenFilePaths.SaveDataFolderPath, "Logs");
    }
}
