using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class JutsuBuild
{
    public static void Windows64()
    {
        var args = Environment.GetCommandLineArgs();
        var index = Array.IndexOf(args, "-jutsuBuildPath");
        if (index < 0 || index + 1 >= args.Length)
            throw new BuildFailedException("Pass -jutsuBuildPath with the output executable path.");

        var path = Path.GetFullPath(args[index + 1]);
        JutsuSafetyChecks.Run();
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray(),
            locationPathName = path,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        });
        Debug.Log($"JUTSU_BUILD_RESULT: {report.summary.result}; errors={report.summary.totalErrors}; " +
                  $"warnings={report.summary.totalWarnings}; bytes={report.summary.totalSize}");
        if (report.summary.result != BuildResult.Succeeded)
            throw new BuildFailedException("JUTSU Windows build failed. See the build log.");
    }
}
