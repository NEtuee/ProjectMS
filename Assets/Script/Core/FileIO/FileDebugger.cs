using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class FileDebugger
{
    public static void OpenFileWithCursor(string filePath, int lineNumber)
    {
        string vscodePath = "code";
        string command = $"{vscodePath} -g {filePath}:{lineNumber}";

        Process.Start("cmd.exe", $"/c {command}");
    }
}
