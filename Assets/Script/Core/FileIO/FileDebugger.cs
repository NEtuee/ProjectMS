using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System.Linq;

public class FileDebugger
{
    public static void OpenFileWithCursor(string filePath, int lineNumber)
    {
        if(Application.platform == RuntimePlatform.OSXEditor)
		{
            // VS Code 실행 파일 경로 (Mac 기준)
            string vscodePath = "/Applications/Visual Studio Code.app/Contents/Resources/app/bin/code";

            // 프로세스를 실행하여 VS Code로 파일 열기
            Process process = new Process();
            process.StartInfo.FileName = vscodePath;
            process.StartInfo.Arguments = $"--goto \"{filePath}:{lineNumber}\""; // 파일 경로를 인자로 전달
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;

            try
            {
                process.Start();
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"Failed to open file in VS Code: {ex.Message}");
            }
        }
        else
        {
            string vscodePath = "code";
            string command = $"{vscodePath} -g {filePath}:{lineNumber}";

            Process.Start("cmd.exe", $"/c {command}");
        }
    }

    public static int findLine(string filePath, string targetString)
    {
        var lines = File.ReadLines(filePath);
        int line = 1;
        foreach(var item in lines)
        {
            if(item.Contains(targetString))
                break;
            ++line;
        }

        return line;
    }
}
