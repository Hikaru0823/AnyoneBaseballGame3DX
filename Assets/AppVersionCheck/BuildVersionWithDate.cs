#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;

public class BuildVersionWithDate : IPreprocessBuildWithReport, IPostprocessBuildWithReport
{
    const string BUILD_DATE_KEY = "build_date_key";
    public static string date = DateTime.Now.ToString("yyyyMMdd");
    private string baseVersion;
    private string baseLuminVersionName;
    private BuildTarget baseTarget;
    /// ビルド前処理
    public void OnPreprocessBuild(BuildReport _report)
    {
        Application.logMessageReceived += OnBuildError;
        baseVersion = PlayerSettings.bundleVersion;
        //バージョンに日付を付与する
        //PlayerSettings.bundleVersion = $"{baseVersion}.{date}";
        //ビルド日を保存する
        Debug.Log(Application.streamingAssetsPath + "\\" + BUILD_DATE_KEY + ".txt");
        try
        {
            if (!Directory.Exists(Application.streamingAssetsPath))
            {
                Directory.CreateDirectory(Application.streamingAssetsPath);
            }
            var sw = new StreamWriter(Application.streamingAssetsPath + "\\" + BUILD_DATE_KEY + ".txt", false, System.Text.Encoding.UTF8);
            sw.WriteLine($"{date}");
            sw.Flush();
            sw.Close();
        }
        catch(Exception e)
        {
            Debug.Log(e);
        }
        //BayatGames.SaveGameFree.SaveGame.Save(BUILD_DATE_KEY, $"{date}");

        baseTarget = _report.summary.platform;
        /*
        //MagicLeapはLumin.versionNameも更新
        if (baseTarget == BuildTarget.Lumin)
        {
            baseLuminVersionName = PlayerSettings.Lumin.versionName;
            PlayerSettings.Lumin.versionName = PlayerSettings.bundleVersion;
        }
        */
    }

    /// ビルド後処理
    public void OnPostprocessBuild(BuildReport _report)
    {
        Restore();
    }


    private void OnBuildError(string condition, string stackTrace, LogType type)
    {
        if (type == LogType.Error)
        {
            Restore();
        }
    }

    //Rollback
    private void Restore()
    {
        Application.logMessageReceived -= OnBuildError;
        /*
        //バージョンを元に戻す
        PlayerSettings.bundleVersion = baseVersion;

        if (baseTarget == BuildTarget.Lumin)
        {
            PlayerSettings.Lumin.versionName = baseLuminVersionName;
        }
        */
        AssetDatabase.SaveAssets();
    }

    // 開発モードか？
    private bool isDevelopment(BuildReport report)
    {
        return (report.summary.options & BuildOptions.Development) != 0;
    }

    ///  実行順
    ///  MagicLeapにおいて、versionNameを動的に設定するために
    ///  MagicLeapManifestBuildProcessorより先に実行したいため-1を設定する
    public int callbackOrder { get { return -1; } }
}

#endif